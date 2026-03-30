// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using Codon.Binary;
using DotNetty.Buffers;

namespace Synesthesia.Engine.Util.Codecs;

public static class ExtraCodecs
{
    public static readonly Vector2Codec VECTOR_2 = new Vector2Codec();
    public static readonly RectangleFCodec RECTANGLE_F = new RectangleFCodec();


    public class Vector2Codec : IBinaryCodec<Vector2>
    {
        public void Write(IByteBuffer buffer, Vector2 value)
        {
            buffer.WriteFloat(value.X);
            buffer.WriteFloat(value.Y);
        }

        public Vector2 Read(IByteBuffer buffer) => new(buffer.ReadFloat(), buffer.ReadFloat());
    }

    public class RectangleFCodec : IBinaryCodec<RectangleF>
    {
        public void Write(IByteBuffer buffer, RectangleF value)
        {
            buffer.WriteFloat(value.Width);
            buffer.WriteFloat(value.Height);
            buffer.WriteFloat(value.X);
            buffer.WriteFloat(value.Y);
        }

        public RectangleF Read(IByteBuffer buffer) =>
            new RectangleF
            (
                buffer.ReadFloat(),
                buffer.ReadFloat(),
                buffer.ReadFloat(),
                buffer.ReadFloat()
            );
    }
}
