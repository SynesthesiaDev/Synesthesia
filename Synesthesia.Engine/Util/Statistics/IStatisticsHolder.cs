// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Synesthesia.Engine.Util.Statistics;

public interface IStatisticsHolder<T>
{
    void Increment(T type);

    void Decrement(T type);

    long Get(T type);

    void Set(T type, int amount);

    void Reset();

}
