// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Util.Bindables;

public class BindableDouble() : Bindable<double>(0.0)
{
    public double Min { get; set; } = double.MinValue;
    public double Max { get; set; } = double.MaxValue;

    public double Default
    {
        get;
        set
        {
            field = value;
            Set(field, IBindable.GLOBAL_EVENT_SOURCE);
        }
    }

    public override void Set(double newValue, BindableEventSource source)
    {
        var oldValue = InternalValue;
        var clamped = Math.Clamp(newValue, Min, Max);

        InternalValue = clamped;
        Listeners.Filter(p => p.Value != source).Keys.ToList().ForEach(listener => listener.Invoke(oldValue, clamped));
    }
}
