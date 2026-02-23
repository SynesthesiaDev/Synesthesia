// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Util;
using Synesthesia.Engine.Components.Barebones;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;

namespace Synesthesia.Engine.Components.Two.DefaultEngineComponents;

public class LabelledSliderBar : CompositeDrawable2d
{
    public required BarebonesSliderBar SliderBar = null!;

    private string label = string.Empty;

    public required string Label
    {
        get => label;
        set
        {
            if (label == value) return;
            label = value;
            if (textDrawable != null) textDrawable.Text = value;
        }
    }

    private Text2d? textDrawable;

    protected override void OnLoading()
    {
        Children =
        [
            new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    textDrawable = new Text2d
                    {
                        Text = label,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        FontSize = 20
                    },

                    SliderBar
                ]
            }
        ];

        SliderBar.Anchor = Anchor.CentreRight;
        SliderBar.Origin = Anchor.CentreRight;
    }
}
