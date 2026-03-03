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
        var currentType = type;

        while (currentType != null && currentType != typeof(object))
        {
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<ResolvedAttribute>() == null)
                    continue;

                var service = DependencyContainer.Get(field.FieldType);
                field.SetValue(target, service);

                EngineStatistics.DEPENDENCIES_RESOLVED_REFLECTION.Increment();
            }

            currentType = currentType.BaseType;
        }
    }

    public static void CheckForDisposing(object target)
    {
        var currentType = target.GetType();

        while (currentType != null && currentType != typeof(object))
        {
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => !Attribute.IsDefined(f, typeof(ExternalOwnershipAttribute)))
                .Where(f => disposing_warning_targets.Any(interfaceType => interfaceType.IsAssignableFrom(f.FieldType)));

            foreach (var field in fields)
            {
                var value = field.GetValue(target);

                if (value is IBindable { IsDisposed: false })
                {
                    Logger.Warning($"Bindable {field.Name} (declared in {currentType.Name}) has not been disposed!", Logger.Runtime);
                }

                if (value is IEventDispatcher { IsPooled: false, IsDisposed: false })
                {
                    Logger.Warning($"EventDispatcher {field.Name} (declared in {currentType.Name}) has not been disposed!", Logger.Runtime);
                }
            }
            currentType = currentType.BaseType;
        }
    }
}
