// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;

namespace Synesthesia.Engine.Audio;

public record AudioDevice(int Index, DeviceInfo BassDeviceInfo)
{
    public int BassId => AudioManager.BASS_INTERNAL_DEVICE_COUNT + Index;
};
