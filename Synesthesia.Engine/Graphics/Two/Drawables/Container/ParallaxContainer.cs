// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Common.Util;
using Synesthesia.Engine.Animations;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Input;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Graphics.Two.Drawables.Container;

public class ParallaxContainer : Container2d
{
    public float Strength { get; set; } = 0.05f;

    public long Duration { get; set; } = 500;

    private readonly Container2d content = new Container2d
    {
        RelativeSizeAxes = Axes.Both,
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre
    };

    public readonly Bindable<bool> Enabled = new(true);

    public readonly Bindable<bool> HoverOnly = new(false);

    private Vector2 lastPosition = Vector2.Zero;

    public IEnumerable<Drawable2d> Content
    {
        get => content.Children;
        set => content.Children = value.ToList();
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        base.OnUpdate(frameInfo);

        if (!Enabled.Value) return;
        if (HoverOnly.Value && !Contains(InputManager.LastMousePosition))
        {
            lastPosition = Vector2.Zero;
        }
        else
        {
            lastPosition = InputManager.LastMousePosition;
        }

        var half = Size / 2;
        var relative = ToLocalSpace(lastPosition) - half;

        relative.X = (float)(Math.Sign(relative.X) * MathUtil.Damp(0, 1, .999f, Math.Abs(relative.X)));
        relative.Y = (float)(Math.Sign(relative.Y) * MathUtil.Damp(0, 1, .999f, Math.Abs(relative.Y)));

        var elapsed = Math.Clamp((int)frameInfo.Delta, 0, Duration);
        content.Position = Transforms.VECTOR2.GetValueAt(elapsed, content.Position, relative * half * Strength, 0, Duration, Easing.Out);
        content.Scale = Transforms.VECTOR2.GetValueAt(elapsed, content.Scale, new Vector2(1 + Math.Abs(Strength)), 0, Duration, Easing.Out);
    }

    protected override void LoadComplete()
    {
        Enabled.OnValueChange(e =>
        {
            if (e.NewValue) return;
            content.MoveTo(Vector2.Zero, Duration, Easing.OutQuint);
            content.ScaleTo(Vector2.One, Duration, Easing.OutQuint);
        });

        base.LoadComplete();
    }

    protected override void OnLoading()
    {
        Masking = true;
        Children = [content];

        base.OnLoading();
    }

    protected override void Dispose(bool isDisposing)
    {
        HoverOnly.Dispose();
        Enabled.Dispose();
        base.Dispose(isDisposing);
    }
}
