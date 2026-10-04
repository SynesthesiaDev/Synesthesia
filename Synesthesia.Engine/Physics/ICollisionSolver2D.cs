// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Timing;

namespace Synesthesia.Engine.Physics;

public interface ICollisionSolver2D
{
    void Step(Span<Drawable2D> children, FrameInfo frameInfo);
}
