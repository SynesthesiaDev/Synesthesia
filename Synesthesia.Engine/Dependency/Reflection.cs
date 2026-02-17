// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using Common.Bindable;
using Common.Logger;
using Common.Statistics;

namespace Synesthesia.Engine.Dependency;

public static class Reflection
{
    private static readonly Type[] disposing_warning_targets =
    [
        typeof(IBindable),
        typeof(IEventDispatcher)
    ];

    public static void ResolveDependencies(object target)
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

    public static void CheckForDisposing(object target)
    {
        var type = target.GetType();

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => disposing_warning_targets.Any(interfaceType => interfaceType.IsAssignableFrom(f.FieldType)));

        foreach (var field in fields)
        {
            if (field.GetValue(target) is IBindable { IsDisposed: false })
            {
                Logger.Warning($"Bindable {field.Name} in {type.Name} has not been disposed before disposing base drawable", Logger.Runtime);
            }

            if (field.GetValue(target) is IEventDispatcher { IsPooled: false, IsDisposed: false })
            {
                Logger.Warning($"EventDispatcher {field.Name} in {type.Name} (not poolable) has not been disposed before disposing base drawable", Logger.Runtime);
            }
        }
    }
}
