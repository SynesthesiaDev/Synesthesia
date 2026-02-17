using System.Collections.Immutable;
using System.Diagnostics;
using Common.Bindable;
using Common.Logger;
using Common.Statistics;
using ManagedBass;
using ManagedBass.Fx;
using ManagedBass.Mix;
using ManagedBass.Wasapi;
using Synesthesia.Engine.Audio.Controls;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Threading.Runners;
using Synesthesia.Engine.Utility;
using SynesthesiaUtil;

namespace Synesthesia.Engine.Audio;

public class AudioManager : BassDspAudioHandler, IHasAudioHandle
{
    public const int PLAYBACK_SAMPLE_RATE = 44100;

    public const int BASS_INTERNAL_DEVICE_COUNT = 2;

    private const int bass_default_device = 1;

    public int MasterMixdownHandle { get; private set; }

    public int GetAudioHandle() => MasterMixdownHandle;

    public readonly DeferredActionQueue DeferredActionQueue = new();

    public void AttachTo(IHasAudioHandle audioHandle) => throw new NotSupportedException();

    // store in a field so it doesn't get GC'd into oblivion
    private WasapiProcedure? wasapiProcedure;
    private WasapiNotifyProcedure? wasapiNotifyProcedure;

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

    private readonly List<AudioChannel> channels = [];

    public IReadOnlyList<AudioChannel> Channels => channels;

    public ImmutableArray<AudioDevice> AudioDevices { get; private set; } = [];

    public readonly Bindable<bool> UseWasapi = new(EngineConfiguration.ExperimentalAudioWasapi);

    private AudioThreadRunner? audioThread;

    public AudioManager()
    {
        UseWasapi.OnValueChange(_ =>
        {
            audioThread ??= DependencyContainer.Get<AudioThreadRunner>();
            audioThread.Schedule(Initialize);
        });
    }

    public AudioDevice CurrentAudioDevice
    {
        get => AudioDevices[Bass.CurrentDevice];
        set
        {
            if (setNewAudioDevice(value))
            {
                Logger.Debug($"BASS Initialized with audio device {CurrentAudioDevice.BassDeviceInfo.Name} ({CurrentAudioDevice.BassDeviceInfo.Type})");
            }
            else
            {
                Logger.Error($"Failed to initialize BASS: {Bass.LastError}");
            }
        }
    }

    private bool alreadyInitialized;

    public void Initialize()
    {
        ThreadSafety.AssertRunningOnAudioThread();
        Logger.Verbose("Trying to initialize BASS..", Logger.Audio);

        UpdateAudioDevices();

        Logger.Debug($"BASS Version:           {Bass.Version}", Logger.Audio);
        Logger.Debug($"BASS FX Version:        {BassFx.Version}", Logger.Audio);
        Logger.Debug($"BASS MIX Version:       {BassMix.Version}", Logger.Audio);
        if (UseWasapi.Value) Logger.Debug($"BASS WASAPI Version:    {BassWasapi.Version}", Logger.Audio);

        if (!alreadyInitialized) CurrentAudioDevice = AudioDevices[bass_default_device];

        ensureMaster();
        DeferredActionQueue.FlushAndSwitchToImmediate();
        alreadyInitialized = true;
        Logger.Debug($"BASS Initialized (WASAPI: {BassWasapi.IsStarted})", Logger.Audio);
    }

    public AudioChannel CreateChannel(string name)
    {
        var channel = new AudioChannel(name);
        AddChannel(channel);
        return channel;
    }

    public void AddChannel(AudioChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (MasterMixdownHandle == 0) throw new InvalidOperationException("AudioManager is not initialized (MasterHandle == 0).");

        if (channels.Contains(channel))
            return;

        channel.AttachTo(this);

        foreach (var mixer in channel.Mixers)
            mixer.AttachTo(channel);

        channels.Add(channel);
    }

    public void UpdateSampleLifetimes()
    {
        for (int i = 0; i < channels.Count; i++)
        {
            var channel = channels[i];
            channel.UpdateSampleLifetimes();
        }
    }

    public float Panning
    {
        get
        {
            Bass.ChannelGetAttribute(MasterMixdownHandle, ChannelAttribute.Pan, out var pan);
            return pan;
        }
        set
        {
            var sanitized = Math.Clamp(value, -1f, 1f);
            Bass.ChannelSetAttribute(MasterMixdownHandle, ChannelAttribute.Pan, sanitized);
        }
    }


    private void ensureMaster()
    {
        if (MasterMixdownHandle != 0)
        {
            EngineStatistics.AUDIO_CHANNELS.Decrement();
            freeWasapi();
            Bass.ChannelStop(MasterMixdownHandle);
            Bass.StreamFree(MasterMixdownHandle);
            MasterMixdownHandle = 0;
        }

        if (UseWasapi.Value)
        {
            if (!tryInitWasapi())
            {
                Logger.Error($"Failed to initialize WASAPI: {Bass.LastError}");
                Logger.Error($"Retrying without WASAPI enabled..");
                UseWasapi.Value = false;
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

        EngineStatistics.AUDIO_CHANNELS.Increment();
        Bass.ChannelPlay(MasterMixdownHandle);

        foreach (var channel in channels)
        {
            channel.AttachTo(this);

            foreach (var mixer in channel.Mixers)
                mixer.AttachTo(channel);
        }

        Panning = Panning;
        Volume = Volume;
    }

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


    private bool tryInitWasapi()
    {
        if (RuntimeInfo.OS != RuntimeInfo.Platform.Windows) return false;

        Logger.Verbose("Trying to initialize WASAPI...", Logger.Audio);

        int wasapiDevice = -1;

        // WASAPI device indices don't match normal BASS devices.
        // Each device is listed multiple times with each supported channel/frequency pair.
        //
        // Working backwards to find the correct device is how bass does things internally (see BassWasapi.GetBassDevice).
        if (Bass.CurrentDevice > 0)
        {
            string driver = Bass.GetDeviceInfo(Bass.CurrentDevice).Driver;

            if (!string.IsNullOrEmpty(driver))
            {
                // In the normal execution case, BassWasapi.GetDeviceInfo will return false as soon as we reach the end of devices.
                // This while condition is just a safety to avoid looping forever.
                // It's intentionally quite high because if a user has many audio devices, this list can get long.
                //
                // Retrieving device info here isn't free. In the future we may want to investigate a better method.
                while (wasapiDevice < 16384)
                {
                    if (!BassWasapi.GetDeviceInfo(++wasapiDevice, out WasapiDeviceInfo info))
                        break;

                    if (info.ID == driver)
                        break;
                }
            }
        }

        return initWasapi(wasapiDevice);
    }

    private bool initWasapi(int wasapiDevice)
    {
        // store in a field so the garbage collector doesn't eat it
        wasapiProcedure = (buffer, length, _) => MasterMixdownHandle == 0 ? 0 : Bass.ChannelGetData(MasterMixdownHandle, buffer, length);

        wasapiNotifyProcedure = (notify, device, _) =>
        {
            if (notify != WasapiNotificationType.DefaultOutput) return;

            freeWasapi();
            initWasapi(device);
        };

        bool initialised = BassWasapi.Init(wasapiDevice, Procedure: wasapiProcedure, Flags: WasapiInitFlags.EventDriven | WasapiInitFlags.AutoFormat, Buffer: 0f, Period: float.Epsilon);
        Logger.Debug($"Initialising BassWasapi for device {wasapiDevice}... {(initialised ? "success!" : "FAILED")}", Logger.Audio);

        if (!initialised)
            return false;

        BassWasapi.GetInfo(out var wasapiInfo);

        MasterMixdownHandle = BassMix.CreateMixerStream(wasapiInfo.Frequency, wasapiInfo.Channels, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
        BassWasapi.Start();
        BassWasapi.SetNotify(wasapiNotifyProcedure);

        return true;
    }


    private void freeWasapi()
    {
        if (MasterMixdownHandle == 0) return;

        Bass.StreamFree(MasterMixdownHandle);
        BassWasapi.Stop();
        BassWasapi.Free();
        MasterMixdownHandle = 0;
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
        freeWasapi();

        // Set latency to sanest minimum
        Bass.DeviceBufferLength = 20;
        Bass.PlaybackBufferLength = 150;

        // Makes the audio device run 24/7 even if there is no audio playing just to make sure there are no delays when staring new audio clip after silence
        Bass.DeviceNonStop = true;

        if (!Bass.Init(device.Index, Flags: (DeviceInitFlags)128)) // 128 == BASS_DEVICE_REINIT
            return false;

        // without this, if bass falls back to directsound legacy mode the audio playback offset will be way off.
        Bass.Configure(ManagedBass.Configuration.TruePlayPosition, 0);

        // Set BASS_IOS_SESSION_DISABLE here to leave session configuration in our hands for later
        Bass.Configure(ManagedBass.Configuration.IOSSession, 16);

        // Always provide a default device
        Bass.Configure(ManagedBass.Configuration.IncludeDefaultDevice, true);

        // Enable a custom BASS_CONFIG_MP3_OLDGAPS flag for backwards compatibility.
        // - This disables support for ItunSMPB tag parsing to match previous expectations.
        // - This also disables a change which assumes a 529 sample (2116 byte in stereo 16-bit) delay if the MP3 file doesn't specify one.
        //   (That was added in Bass for more consistent results across platforms and standard/mp3-free BASS versions, because OSX/iOS's MP3 decoder always removes 529 samples)
        Bass.Configure((ManagedBass.Configuration)68, 1);

        // Disable BASS_CONFIG_DEV_TIMEOUT flag to keep BASS audio output from pausing on device processing timeout.
        // See https://www.un4seen.com/forum/?topic=19601 for more information.
        Bass.Configure((ManagedBass.Configuration)70, false);

        ensureMaster();

        return true;
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
}
