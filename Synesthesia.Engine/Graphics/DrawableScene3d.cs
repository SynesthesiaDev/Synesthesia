// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Graphics.Three;

namespace Synesthesia.Engine.Graphics;

public class DrawableScene3d : CompositeDrawable3d
{
    private Camera3d? camera;

    public Camera3d? ActiveCamera3d
    {
        get => camera;
        set
        {
            if (value != null && !GetFlattenedChildrenList().Contains(value))
                throw new InvalidOperationException("Camera must be part of the scene hierarchy");

            camera = value;
        }
    }
}
