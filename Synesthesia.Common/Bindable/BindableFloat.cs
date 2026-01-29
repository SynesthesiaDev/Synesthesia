// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Common.Bindable;

public class BindableFloat(float defaultInternalValue = 0f) : Bindable<float>(defaultInternalValue)
{
    public float Min { get; set; } = float.MinValue;
    public float Max { get; set; } = float.MaxValue;

    public new float Value
    {
        get => InternalValue;
        set
        {
            var oldValue = InternalValue;
            var clamped = Math.Clamp(value, Min, Max);
            if(Math.Abs(clamped - oldValue) < 0f) return;

            InternalValue = value;
            Listeners.ForEach(listener => listener.Invoke(oldValue, value));
        }
    }
}
