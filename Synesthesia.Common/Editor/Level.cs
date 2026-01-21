using Codon.Binary;

namespace Common.Editor;

public record Level(string DisplayName, string Author)
{
    public static IBinaryCodec<Level> Codec = BinaryCodec.Of(
        BinaryCodec.String, l => l.DisplayName,
        BinaryCodec.String, l => l.Author,
        (name, author) => new Level(name, author)
    );
}