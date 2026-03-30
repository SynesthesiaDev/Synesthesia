// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using DotNetty.Buffers;

namespace Synesthesia.Engine.Extensions;

public static class ByteArrayExtensions
{
    public static IByteBuffer ToByteBuffer(this byte[] byteArray) => Unpooled.CopiedBuffer(byteArray);
}
