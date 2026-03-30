// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using DotNetty.Buffers;

namespace Synesthesia.Engine.Extensions;

public static class StreamExtensions
{
    extension(Stream stream)
    {
        public byte[] ToByteArray()
        {
            byte[] data;

            if (stream is MemoryStream ms)
            {
                data = ms.ToArray();
            }
            else
            {
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                data = memoryStream.ToArray();
            }

            return data;
        }

        public IByteBuffer ToByteBuffer() => Unpooled.CopiedBuffer(stream.ToByteArray());
    }
}
