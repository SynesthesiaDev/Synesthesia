using System.ComponentModel;
using Veldrid;

namespace Synesthesia.Engine.Configuration;

public enum RendererType
{
    [Description("Automatic")] Automatic,
    [Description("Metal")] Metal,
    [Description("Vulkan")] Vulkan,
    [Description("Direct3D 11")] Direct3D11,
    [Description("OpenGL")] OpenGL,
    [Description("OpenGL (Legacy)")] OpenGLLegacy,
}

public static class RendererTypeInfo
{
    public static GraphicsBackend GetGraphicsBackend(RendererType type)
    {
        return type switch
        {
            RendererType.Automatic => GraphicsBackend.Vulkan,
            RendererType.Metal => GraphicsBackend.Metal,
            RendererType.Vulkan => GraphicsBackend.Vulkan,
            RendererType.Direct3D11 => GraphicsBackend.Direct3D11,
            RendererType.OpenGL => GraphicsBackend.OpenGL,
            RendererType.OpenGLLegacy => GraphicsBackend.OpenGLES,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}