using Codon.Buffer;
using Common.Bindable;
using Common.Event;

namespace Synesthesia.Engine.Audio;

public class AudioSample
{
    public required string Name;
    public required double Lenght;
    public required BinaryBuffer Data;

    public AudioSample()
    {
    }


}