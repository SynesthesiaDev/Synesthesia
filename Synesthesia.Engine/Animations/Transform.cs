// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Engine.Animations.Easings;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Animations;

public abstract class Transform<T>
{
    public abstract T Apply(T startValue, T endValue, float progress);

    public T GetValueAt(float time, T start, T end, float startTime, float endTime, Easing easing)
    {
        var easingFunction = new EasingFunction(easing);

        float progress = (time - startTime) / (endTime - startTime);
        progress = easingFunction.ApplyEasing(progress).ToFloat();

        progress = Math.Clamp(progress, 0f, 1f);
        return Apply(start, end, progress);
    }
}
