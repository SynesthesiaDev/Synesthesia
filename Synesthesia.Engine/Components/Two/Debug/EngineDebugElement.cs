// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Text;
using Synesthesia.Engine.Util;

namespace Synesthesia.Engine.Components.Two.Debug;

public abstract class EngineDebugElement : CompositeDrawable2D
{
    protected class HeaderComponent(string name) : CompositeDrawable2D
    {
        protected override void OnLoading()
        {
            Size = new Vector2(270, 30);
            Children =
            [
                new Text2D
                {
                    Text = name,
                    Weight = FontWeight.Bold,
                    Color = EngineBranding.TEXT2
                },
            ];
        }
    }
}
