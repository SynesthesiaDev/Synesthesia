// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using Synesthesia.Engine.Components.Barebones;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class DefaultSliderBar : BarebonesSliderBar
{
    protected override SliderBarBody GetBody() => new DefaultSliderBarBody();

    protected override SliderBarNub GetNub() => new DefaultSliderBarNub(this);
}
