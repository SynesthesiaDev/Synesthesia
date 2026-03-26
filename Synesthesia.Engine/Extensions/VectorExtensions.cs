// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Numerics;

namespace Synesthesia.Engine.Extensions;

public static class VectorExtensions
{
    extension(Vector2 vector)
    {
        public string AsString() => string.Create(CultureInfo.InvariantCulture, $"{vector.X}x{vector.Y}");
    }

}
