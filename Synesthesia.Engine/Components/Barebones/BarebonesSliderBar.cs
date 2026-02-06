// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Common.Bindable;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;

namespace Synesthesia.Engine.Components.Barebones;

public abstract class BarebonesSliderBar : Container2d
{
    public required BindableFloat Current { get; init; }

    private readonly BindableEventSource selfEventSource = new();

    public float Precision { get; set; } = 0;

    protected abstract SliderBarBody GetBody();
    protected abstract SliderBarNub GetNub();

    private SliderBarBody body = null!;
    private SliderBarNub nub = null!;

    public void UpdateFromPositionalInput(Vector2 mousePos)
    {
        var localSpacePosX = ToLocalSpace(mousePos).X - nub.Size.X / 2;
        var clamped = Math.Clamp(localSpacePosX, 0, Size.X - (nub.Size.X));

        var progress = clamped / (Size.X - nub.Size.X);
        var rawValue = Current.Min + (progress * (Current.Max - Current.Min));

        float finalValue;
        if (Precision > 0)
        {
            var decPrecision = decimal.CreateTruncating(Precision);
            decimal accurateResult = decimal.CreateTruncating(rawValue);
            accurateResult = Math.Round(accurateResult / decPrecision) * decPrecision;
            finalValue = float.CreateTruncating(accurateResult);
        }
        else
        {
            finalValue = rawValue;
        }

        Current.Set(finalValue, selfEventSource);

        // Snap nub to the actual value position
        var snappedProgress = (finalValue - Current.Min) / (Current.Max - Current.Min);
        var newNubPos = nub.Position with { X = snappedProgress * (Size.X - nub.Size.X) };
        nub.MoveTo(newNubPos, 10, Easing.OutQuart);

        body.ValueChanged(snappedProgress);
    }

    protected override void LoadComplete()
    {
        Current.OnValueChange(e =>
        {
            var progress = (e.NewValue - Current.Min) / (Current.Max - Current.Min);

            var newNubPos = nub.Position with { X = progress * (Size.X - nub.Size.X) };
            nub.MoveTo(newNubPos, 10, Easing.OutQuart);

            body.ValueChanged(progress);

        }, true, selfEventSource);
    }

    protected override void OnLoading()
    {
        Children =
        [
            body = GetBody(),
            nub = GetNub()
        ];
    }
}
