// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Timing;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class ParallaxContainer : Container2d
{
    private const int parallax_duration = 500;

    public float Strength { get; set; } = 0.05f;

    private Container2d content = null!;

    public readonly Bindable<bool> Enabled = new(true);

    private readonly StopwatchClock stopwatchClock = new(false);

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);

        if (!Enabled.Value) return;

        var half = Size / 2;
        var relative = ToLocalSpace(InputManager.LastMousePosition) - half;

        relative.X = (float)(Math.Sign(relative.X) * MathUtil.Damp(0, 1, .999f, Math.Abs(relative.X)));
        relative.Y = (float)(Math.Sign(relative.Y) * MathUtil.Damp(0, 1, .999f, Math.Abs(relative.Y)));

        var elapsed = Math.Clamp(stopwatchClock.ElapsedMilliseconds, 0, parallax_duration);
        content.Position = Transforms.VECTOR2.GetValueAt(elapsed, content.Position, relative * half * Strength, 0, parallax_duration, Easing.Out);
        content.Scale = Transforms.VECTOR2.GetValueAt(elapsed, content.Scale, new Vector2(1 + Math.Abs(Strength)), 0, parallax_duration, Easing.Out);
    }

    protected override void LoadComplete()
    {
        Enabled.OnValueChange(e =>
        {
            if (!e.NewValue) return;
            content.MoveTo(Vector2.Zero, parallax_duration, Easing.OutQuint);
            content.ScaleTo(Vector2.Zero, parallax_duration, Easing.OutQuint);
        });

        base.LoadComplete();
    }

    protected override void OnLoading()
    {
        Children =
        [
            content = new Container2d
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            }
        ];

        base.OnLoading();
    }
}
