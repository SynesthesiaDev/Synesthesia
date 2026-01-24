using Codon.Buffer;
using SynesthesiaUtil.Extensions;
using SynesthesiaUtil.Types;

namespace Synesthesia.Engine.Audio;

public record AudioSample : IDisposable
{
    public readonly string Name;
    
    public readonly double Lenght;
    
    public double RestartPoint { get; set; } = 0.0;

    public virtual bool Looping { get; set; } = false;
    
    public readonly BinaryBuffer Data;

    public virtual int? Bitrate => null;
    
    public FloatRange DefaultVolume { get; set; } = new(1f, 1f);
    
    public FloatRange DefaultPitch { get; set; } = new(1f, 1f);
    
    public AudioSample(string name, BinaryBuffer data)
    {
        Name = name;
        Data = data;
        Lenght = Data.Length / AudioManager.PLAYBACK_SAMPLE_RATE.ToDouble();
    }

    public void Dispose()
    {
        Data.Dispose();
    }
}