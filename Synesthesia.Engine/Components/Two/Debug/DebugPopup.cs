// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Layout;
using Synesthesia.Engine.Graphics.Two;
using Synesthesia.Engine.Graphics.Two.Container;
using Synesthesia.Engine.Graphics.Two.Text;
using Synesthesia.Engine.Util;

namespace Synesthesia.Engine.Components.Two.Debug;

public class DebugPopup : CompositeDrawable2D
{
    private Text2D titleDrawable = null!;
    private Text2D textDrawable = null!;

    protected override void OnLoading()
    {
        AutoSizeAxes = Axes.Both;
        Children =
        [
            new Box2D
            {
                RelativeSizeAxes = Axes.Both,
                Color = EngineBranding.BACKGROUND2,
                CornerRadius = 10,
            },

            new Container2D
            {
                AutoSizeAxes = Axes.Both,
                AutoSizePadding = new Vector4(15),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new FillFlowContainer2D
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = Direction.Vertical,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Spacing = 10,
                        Children =
                        [
                            titleDrawable = new Text2D
                            {
                                Color = EngineBranding.TEXT2,
                                Text = "Title",
                                Weight = FontWeight.Bold,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },

                            textDrawable = new Text2D
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Color = EngineBranding.TEXT1,
                                Text = "Texting",
                            },
                        ]
                    },
                ],
            },
        ];
    }

    public void Push(string title, string text)
    {
        titleDrawable.Text = title;
        textDrawable.Text = text;
        Animator.Value.Clear();

        if (!Visible)
        {
            Alpha = 0f;
            Visible = true;
        }

        FadeTo(1f, 100, Easing.OutQuart).Then(() => FadeTo(0, 2000, Easing.InCubic).ThenHide(this));
    }
}
