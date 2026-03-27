// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Synesthesia.Engine.Graphics;
using Synesthesia.Engine.Logging;
using Synesthesia.Engine.Util.Bindables;

namespace Synesthesia.Engine.Dependency;

public static class Reflection
{
    private static readonly Type[] disposing_warning_targets =
    [
        typeof(IBindable),
        typeof(IEventDispatcher),
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
                if (field.GetCustomAttribute<SingletonAttribute>() == null)
                    continue;

                var service = DependencyContainer.Get(field.FieldType);
                field.SetValue(target, service);
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

                switch (value)
                {
                    case IBindable { IsDisposed: false }:
                        Logger.Warning($"Bindable {field.Name} (declared in {currentType.Name}) has not been disposed!", Logger.Runtime);
                        break;
                    case IEventDispatcher { IsPooled: false, IsDisposed: false }:
                        Logger.Warning($"EventDispatcher {field.Name} (declared in {currentType.Name}) has not been disposed!", Logger.Runtime);
                        break;
                }
            }
            currentType = currentType.BaseType;
        }
    }

    public static unsafe void SetupVertexAttributes<T>(GL gl) where T : unmanaged
    {
        var type = typeof(T);
        var stride = (uint)sizeof(T);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var fieldInfo in fields)
        {
            var vertexInfoAttribute = fieldInfo.GetCustomAttribute<VertexInfoAttribute>();
            if(vertexInfoAttribute == null) continue;

            var offset = Marshal.OffsetOf<T>(fieldInfo.Name);

            gl.EnableVertexAttribArray((uint)vertexInfoAttribute.Index);
            gl.VertexAttribPointer((uint)vertexInfoAttribute.Index, vertexInfoAttribute.Count, vertexInfoAttribute.Type, vertexInfoAttribute.Normalized, stride, (void*)offset);
        }
    }
}
