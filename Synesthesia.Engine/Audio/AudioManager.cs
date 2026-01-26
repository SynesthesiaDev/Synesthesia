using System.Collections.Immutable;
using System.Diagnostics;
using Common.Logger;
using Common.Statistics;
using ManagedBass;
using ManagedBass.Fx;
using ManagedBass.Mix;
using ManagedBass.Wasapi;
using Synesthesia.Engine.Audio.Controls;
using Synesthesia.Engine.Threading;
using Synesthesia.Engine.Utility;
using SynesthesiaUtil;

namespace Synesthesia.Engine.Audio;

public class AudioManager : DeferredActionQueue, IHasAudioHandle
{
    public const int PLAYBACK_SAMPLE_RATE = 44100;

    public const int BASS_INTERNAL_DEVICE_COUNT = 2;

    private const int bass_default_device = 1;

    public int MasterMixdownHandle { get; private set; }

    public int GetAudioHandle() => MasterMixdownHandle;

    public void AttachTo(IHasAudioHandle audioHandle) => throw new NotSupportedException();

    public float MasterVolume
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

    public void Initialize()
    {
        ThreadSafety.AssertRunningOnAudioThread();
        Logger.Verbose("Trying to initialize BASS..", Logger.Audio);

        UpdateAudioDevices();

        Logger.Debug($"BASS Version:        {Bass.Version}", Logger.Audio);
        Logger.Debug($"BASS FX Version:     {BassFx.Version}", Logger.Audio);
        Logger.Debug($"BASS MIX Version:    {BassMix.Version}", Logger.Audio);

        CurrentAudioDevice = AudioDevices[bass_default_device];

        ensureMaster();
        FlushAndSwitchToImmediate();
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

    public void UpdateSampleLifetimes() => channels.ForEach(channel => channel.UpdateSampleLifetimes());


    private void ensureMaster()
    {
        if (MasterMixdownHandle != 0)
        {
            EngineStatistics.AUDIO_CHANNELS.Decrement();
            Bass.ChannelStop(MasterMixdownHandle);
            Bass.StreamFree(MasterMixdownHandle);
            MasterMixdownHandle = 0;
        }

        MasterMixdownHandle = BassMix.CreateMixerStream(PLAYBACK_SAMPLE_RATE, 2, BassFlags.MixerNonStop | BassFlags.Float);
        if (MasterMixdownHandle == 0)
            throw new InvalidOperationException($"Failed to create master mixer: {Bass.LastError}");

        MasterVolume = 1f;

        EngineStatistics.AUDIO_CHANNELS.Increment();
        Bass.ChannelPlay(MasterMixdownHandle);
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


    private bool setNewAudioDevice(AudioDevice device)
    {
        ThreadSafety.AssertRunningOnAudioThread();
        Trace.Assert(device.Index != -1);

        if (device.Index >= Bass.DeviceCount) throw new InvalidOperationException($"Invalid audio device index: {device.Index}");

        if (Bass.CurrentDevice == device.Index) return false;

        if (!device.BassDeviceInfo.IsEnabled) throw new InvalidOperationException("Audio device is not enabled!");

        if (device.BassId == Bass.NoSoundDevice) return false;

        if (AudioDevices.IsEmpty || RuntimeInfo.IsMobile) return false;

        if (!Bass.Init(device.Index, Flags: (DeviceInitFlags)128)) // 128 == BASS_DEVICE_REINIT
            return false;

        // TODO wasapi support
        BassWasapi.Stop();
        BassWasapi.Free();

        // Set latency to sanest minimum
        Bass.DeviceBufferLength = 10;
        Bass.PlaybackBufferLength = 100;

        // Makes the audio device run 24/7 even if there is no audio playing just to make sure there are no delays when staring new audio clip after silence
        Bass.DeviceNonStop = true;

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

        foreach (var channel in channels)
        {
            channel.AttachTo(this);

            foreach (var mixer in channel.Mixers)
                mixer.AttachTo(channel);
        }

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
