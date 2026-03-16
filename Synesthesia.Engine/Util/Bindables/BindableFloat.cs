// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Util.Bindables;

public class BindableFloat() : Bindable<float>(0.0f)
{
    public float Min { get; set; } = float.MinValue;
    public float Max { get; set; } = float.MaxValue;

    private float defaultValue;

    public float Default
    {
        get => defaultValue;
        set
        {
            defaultValue = value;
            Set(defaultValue, IBindable.GLOBAL_EVENT_SOURCE);
        }
    }

    public override void Set(float newValue, BindableEventSource source)
    {
        var oldValue = InternalValue;
        var clamped = Math.Clamp(newValue, Min, Max);

        InternalValue = clamped;
        Listeners.Filter(p => p.Value != source).Keys.ToList().ForEach(listener => listener.Invoke(oldValue, clamped));
    }

    // public new void Set(float newValue, BindableEventSource source)
    // {
        // var oldValue = InternalValue;
        // var clamped = Math.Clamp(newValue, Min, Max);

        // InternalValue = clamped;
        // Listeners.Filter(p => p.Value != source).Keys.ToList().ForEach(listener => listener.Invoke(oldValue, clamped));
    // }

}
