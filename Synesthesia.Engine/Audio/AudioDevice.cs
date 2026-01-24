using ManagedBass;

namespace Synesthesia.Engine.Audio;

public record AudioDevice(int Index, DeviceInfo BassDeviceInfo)
{
    public int BassId => AudioManager.BASS_INTERNAL_DEVICE_COUNT + Index;
};
