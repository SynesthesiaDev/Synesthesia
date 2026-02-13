// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using Common.Statistics;

namespace Synesthesia.Engine.Dependency;

public static class DependencyInjector
{
    public static void Inject(object target)
    {
        var type = target.GetType();

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<ResolvedAttribute>() != null);

        foreach (var field in fields)
        {
            var service = DependencyContainer.Get(field.FieldType);
            EngineStatistics.DEPENDENCIES_RESOLVED_REFLECTION.Increment();
            field.SetValue(target, service);
        }
    }
}
