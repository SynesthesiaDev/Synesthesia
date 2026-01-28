// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Two;

namespace Synesthesia.VisualTests;

public abstract class VisualTest
{
    public abstract string Name { get; }

    public abstract List<Drawable2d> Setup();

    public abstract void Cleanup();

}
