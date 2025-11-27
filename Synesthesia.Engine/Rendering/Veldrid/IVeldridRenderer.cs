using Veldrid;

namespace Synesthesia.Engine.Rendering.Veldrid;

public interface IVeldridRenderer : IRenderer
{
    GraphicsDevice Device { get; }

    ResourceFactory ResourceFactory { get; }

    bool UseStructuredBuffers { get; }

}