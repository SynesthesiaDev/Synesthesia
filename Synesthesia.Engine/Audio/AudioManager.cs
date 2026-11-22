// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using ManagedBass;
using ManagedBass.Fx;
using ManagedBass.Mix;
using ManagedBass.Wasapi;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Threading.Threads;
using Synesthesia.Engine.Util.Statistics;
using Synesthesia.Utils;
using System.Runtime.InteropServices;

namespace Synesthesia.Engine.Audio;

public class AudioManager : BassAudioHandler, IHasAudioHandle
{
    public const int PLAYBACK_SAMPLE_RATE = 44100;
    public const int BASS_INTERNAL_DEVICE_COUNT = 2;
    private const int bass_default_device = 1;

    private bool initialized;
    private AudioThread? audioThread;
    private readonly ConcurrentQueue<Action> commandQueue = new();
    private readonly Wasapi wasapi;

    public int MasterMixdownHandle { get; set; }

    public int GetAudioHandle() => MasterMixdownHandle;

    public void AttachTo(IHasAudioHandle audioHandle) => throw new NotSupportedException("Cannot attach master audio bus to another audio bus");

    public override float Volume
    {
        get
        {
            if (MasterMixdownHandle == 0) return 0f;
            Bass.ChannelGetAttribute(MasterMixdownHandle, ChannelAttribute.Volume, out var volume);
            return volume;
        }
        set
        {
            if (MasterMixdownHandle == 0) return;
            Bass.ChannelSetAttribute(MasterMixdownHandle, ChannelAttribute.Volume, value);
        }
    }

    public float Panning
    {
        get
        {
            Bass.ChannelGetAttribute(MasterMixdownHandle, ChannelAttribute.Pan, out var pan);
            return pan;
        }
        set => Bass.ChannelSetAttribute(MasterMixdownHandle, ChannelAttribute.Pan, Math.Clamp(value, -1f, 1f));
    }

    private readonly List<AudioBus> buses = [];

    public IReadOnlyList<AudioBus> Buses => buses;

    public ImmutableArray<AudioDevice> AudioDevices { get; private set; } = [];

    public bool UseWasapi
    {
        get;
        set
        {
            field = value;
            Schedule(Initialize);
        }
    } = true;

    public AudioDevice CurrentAudioDevice
    {
        get => AudioDevices[Bass.CurrentDevice];
        set
        {
            if (setNewAudioDevice(value))
            {
                Logger.Debug($"BASS Initialized with audio device {CurrentAudioDevice.BassDeviceInfo.Name} ({CurrentAudioDevice.BassDeviceInfo.Type})", Logger.Audio);
            }
            else
            {
                Logger.Error($"Failed to initialize BASS: {Bass.LastError}", Logger.Audio);
            }
        }
    }

    public AudioManager()
    {
        audioThread = DependencyContainer.Get<AudioThread>();
        wasapi = new Wasapi(this);
    }

    #region Command Queue

    private void flushCommandQueue()
    {
        while (commandQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    public void Schedule(Action action) => commandQueue.Enqueue(action);

    #endregion

    #region Init

    public void Initialize()
    {
        ThreadSafety.AssertRunningOnAudioThread();
        Logger.Verbose("Trying to initialize BASS..", Logger.Audio);

        UpdateAudioDevices();

        Logger.Debug($"BASS Version:           {Bass.Version}", Logger.Audio);
        Logger.Debug($"BASS FX Version:        {BassFx.Version}", Logger.Audio);
        Logger.Debug($"BASS MIX Version:       {BassMix.Version}", Logger.Audio);
        if (UseWasapi) Logger.Debug($"BASS WASAPI Version:    {BassWasapi.Version}", Logger.Audio);

        if (!initialized) CurrentAudioDevice = AudioDevices[bass_default_device];

        ensureMasterInitialized();
        flushCommandQueue();
        initialized = true;
        Logger.Debug($"BASS Initialized", Logger.Audio);
    }

    private void ensureMasterInitialized()
    {
        if (MasterMixdownHandle != 0)
        {
            EngineStatistics.Decrement(EngineStatistics.Type.AudioBuses);
            wasapi.Free();
            Bass.ChannelStop(MasterMixdownHandle);
            Bass.StreamFree(MasterMixdownHandle);
            MasterMixdownHandle = 0;
        }

        if (UseWasapi)
        {
            if (!wasapi.TryInitialize())
            {
                Logger.Error($"Failed to initialize WASAPI: {Bass.LastError}", Logger.Audio);
                Logger.Error("Retrying without WASAPI enabled..", Logger.Audio);
                UseWasapi = false;
                Initialize();
            }
        }
        else
        {
            MasterMixdownHandle = BassMix.CreateMixerStream(PLAYBACK_SAMPLE_RATE, 2, BassFlags.MixerNonStop | BassFlags.Float);
        }

        if (MasterMixdownHandle == 0)
            throw new InvalidOperationException($"Failed to create master mixer: {Bass.LastError}");

        Volume = 1f;

        AttachDspHandle(MasterMixdownHandle);

        EngineStatistics.Increment(EngineStatistics.Type.AudioBuses);
        Bass.ChannelPlay(MasterMixdownHandle);

        foreach (ref AudioBus bus in CollectionsMarshal.AsSpan(buses))
        {
            bus.AttachTo(this);

            foreach (var mixer in bus.Mixers)
                mixer.AttachTo(bus);
        }

        Panning = Panning;
        Volume = Volume;
    }

    private bool setNewAudioDevice(AudioDevice device)
    {
        ThreadSafety.AssertRunningOnAudioThread();
        Trace.Assert(device.Index != -1);

        if (device.Index >= Bass.DeviceCount) throw new InvalidOperationException($"Invalid audio device index: {device.Index}");

        if (Bass.CurrentDevice == device.Index)
        {
            return false;
        }

        if (!device.BassDeviceInfo.IsEnabled) throw new InvalidOperationException("Audio device is not enabled!");

        if (device.BassId == Bass.NoSoundDevice)
        {
            Logger.Verbose("bass is no sound device");
            return false;
        }

        if (AudioDevices.IsEmpty || RuntimeInfo.IsMobile)
        {
            Logger.Verbose("audio empty | is mobile");
            return false;
        }

        Bass.Stop();
        Bass.Free();
        wasapi.Free();

        // Set latency to the sanest minimum
        Bass.DeviceBufferLength = 20;
        Bass.PlaybackBufferLength = 150;

        // Makes the audio device run 24/7 even if there is no audio playing just to make sure there are no delays when staring new audio clip after silence
        Bass.DeviceNonStop = true;

        if (!Bass.Init(device.Index, Flags: (DeviceInitFlags)128)) // 128 == BASS_DEVICE_REINIT
            return false;

        // without this, if bass falls back to directsound legacy mode the audio playback offset will be way off.
        Bass.Configure(Configuration.TruePlayPosition, 0);

        // Set BASS_IOS_SESSION_DISABLE here to leave session configuration in our hands for later
        Bass.Configure(Configuration.IOSSession, 16);

        // Always provide a default device
        Bass.Configure(Configuration.IncludeDefaultDevice, true);

        // Enable a custom BASS_CONFIG_MP3_OLDGAPS flag for backwards compatibility.
        // - This disables support for ItunSMPB tag parsing to match previous expectations.
        // - This also disables a change which assumes a 529 sample (2116 byte in stereo 16-bit) delay if the MP3 file doesn't specify one.
        //   (That was added in Bass for more consistent results across platforms and standard/mp3-free BASS versions, because OSX/iOS's MP3 decoder always removes 529 samples)
        Bass.Configure((Configuration)68, 1);

        // Disable BASS_CONFIG_DEV_TIMEOUT flag to keep BASS audio output from pausing on device processing timeout.
        // See https://www.un4seen.com/forum/?topic=19601 for more information.
        Bass.Configure((Configuration)70, false);

        ensureMasterInitialized();

        return true;
    }

    #endregion

    #region Bus management

    public AudioBus CreateBus(string name)
    {
        var channel = new AudioBus(name);
        AddBus(channel);
        return channel;
    }

    public void AddBus(AudioBus channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (MasterMixdownHandle == 0) throw new InvalidOperationException("AudioManager is not initialized");

        if (buses.Contains(channel))
            return;

        channel.AttachTo(this);

        foreach (var mixer in channel.Mixers)
            mixer.AttachTo(channel);

        buses.Add(channel);
    }

    #endregion

    #region Updating Devices

    public bool CheckForDeviceChanges()
    {
        var previousDevices = AudioDevices;
        var deviceCount = Bass.DeviceCount;

        if (previousDevices.Length != deviceCount)
            return true;

        for (var i = 0; i < deviceCount; i++)
        {
            var prevInfo = previousDevices[i];

            Bass.GetDeviceInfo(i, out var info);

            if (info.IsEnabled != prevInfo.BassDeviceInfo.IsEnabled)
                return true;

            if (info.IsDefault != prevInfo.BassDeviceInfo.IsDefault)
                return true;
        }

        return false;
    }


    public void UpdateAudioDevices()
    {
        AudioDevices = getAllAudioDevices();
    }

    private static ImmutableArray<AudioDevice> getAllAudioDevices()
    {
        var deviceCount = Bass.DeviceCount;

        var devices = ImmutableArray.CreateBuilder<AudioDevice>(deviceCount);
        for (var i = 0; i < deviceCount; i++)
        {
            devices.Add(new AudioDevice(i, Bass.GetDeviceInfo(i)));
        }

        return devices.MoveToImmutable();
    }

    #endregion

    public void UpdateSoundLifetimes()
    {
        for (int i = 0; i < buses.Count; i++)
        {
            var bus = buses[i];
            bus.UpdateSoundLifetimes();
        }
    }
}
