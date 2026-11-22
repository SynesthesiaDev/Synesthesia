// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using ManagedBass.Mix;
using ManagedBass.Wasapi;
using Synesthesia.Engine.Logging;
using Synesthesia.Utils;

namespace Synesthesia.Engine.Audio;

public class Wasapi(AudioManager audioManager)
{
    // store in a field so it doesn't get eaten by the garbage collector for breakfast
    private WasapiProcedure? wasapiProcedure;
    private WasapiNotifyProcedure? wasapiNotifyProcedure;

    public bool TryInitialize()
    {
        if (RuntimeInfo.OperatingSystem != RuntimeInfo.Platform.Windows) return false;

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

                    if (string.Equals(info.ID, driver, StringComparison.Ordinal))
                        break;
                }
            }
        }

        return initWasapi(wasapiDevice);
    }

    private bool initWasapi(int wasapiDevice)
    {
        // store in a field so the garbage collector doesn't eat it
        wasapiProcedure = (buffer, length, _) => audioManager.MasterMixdownHandle == 0 ? 0 : Bass.ChannelGetData(audioManager.MasterMixdownHandle, buffer, length);

        wasapiNotifyProcedure = (notify, device, _) =>
        {
            if (notify != WasapiNotificationType.DefaultOutput) return;

            Free();
            initWasapi(device);
        };

        bool initialised = BassWasapi.Init(wasapiDevice, Procedure: wasapiProcedure, Flags: WasapiInitFlags.EventDriven | WasapiInitFlags.AutoFormat, Buffer: 0f, Period: float.Epsilon);
        Logger.Debug($"Initialising BassWasapi for device {wasapiDevice}... {(initialised ? "success!" : "FAILED")}", Logger.Audio);

        if (!initialised)
            return false;

        BassWasapi.GetInfo(out var wasapiInfo);

        audioManager.MasterMixdownHandle = BassMix.CreateMixerStream(wasapiInfo.Frequency, wasapiInfo.Channels, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
        BassWasapi.Start();
        BassWasapi.SetNotify(wasapiNotifyProcedure);

        return true;
    }


    public void Free()
    {
        if (audioManager.MasterMixdownHandle == 0) return;

        Bass.StreamFree(audioManager.MasterMixdownHandle);
        BassWasapi.Stop();
        BassWasapi.Free();
        audioManager.MasterMixdownHandle = 0;
    }
}
