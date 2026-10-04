// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Util.Pooling;

namespace Synesthesia.Engine.Physics;

public class PhysicsAndCollisionContainer2D : Container2D
{
    public IPhysicsSolver2D? PhysicsSolver = null;
    public ICollisionSolver2D? CollisionSolver = null;

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);

        Snapshot<Drawable2D> snapshot;
        lock (ChildrenLock)
        {
            snapshot = Snapshot.Rent(InternalChildren);
        }

        using (snapshot)
        {
            foreach (ref Drawable2D child in snapshot.Span)
            {
                if (child is IHasPhysics2D physicsObject)
                {
                    PhysicsSolver?.Step(physicsObject, child, frameInfo);
                }
            }

            CollisionSolver?.Step(snapshot.Span, frameInfo);
        }

    }
}
