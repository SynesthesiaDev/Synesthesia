// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Util;
using Synesthesia.Engine.Components.Two.DefaultEngineComponents;

namespace Synesthesia.VisualTests.Tests;

public class TextboxTest : VisualTest
{
    protected override void OnLoading()
    {
        Children =
        [

            new DefaultTextbox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200, 40)
            }
        ];
    }
}
