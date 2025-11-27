// using Common.Logger;
// using Synesthesia.Engine.Configuration;
// using Synesthesia.Engine.Host;
// using Veldrid;
// using Veldrid.StartupUtilities;
//
// namespace Synesthesia.Engine.Rendering.Veldrid;
//
// public class VeldridDevice
// {
//     public GraphicsDevice Device { get; }
//
//     public ResourceFactory Factory => Device.ResourceFactory;
//
//     public bool VSync
//     {
//         get => Device.SyncToVerticalBlank;
//         set => Device.SyncToVerticalBlank = value;
//     }
//
//     public bool IsDepthRangeZeroToOne => Device.IsDepthRangeZeroToOne;
//     public bool IsUvOriginTopLeft => Device.IsUvOriginTopLeft;
//     public bool IsClipSpaceYInverted => Device.IsClipSpaceYInverted;
//
//     public VeldridDevice(RendererType type, IHost host, WindowCreateInfo windowCreateInfo)
//     {
//         //TODO do not init on draw thread
//
//         var options = new GraphicsDeviceOptions
//         {
//             HasMainSwapchain = true,
//             SwapchainDepthFormat = PixelFormat.R16_UNorm,
//             SyncToVerticalBlank = true,
//             ResourceBindingModel = ResourceBindingModel.Improved
//         };
//
//         var swapChain = new SwapchainDescription
//         {
//             Width = (uint)host.GetWindowSize().X,
//             Height = (uint)host.GetWindowSize().Y,
//             ColorSrgb = options.SwapchainSrgbFormat,
//             DepthFormat = options.SwapchainDepthFormat,
//             SyncToVerticalBlank = options.SyncToVerticalBlank
//         };
//
//         switch (type)
//         {
//             case RendererType.Automatic:
//                 Device = GraphicsDevice.CreateVulkan(options, swapChain); //TODO actually automatic
//                 break;
//             case RendererType.Metal:
//                 Device = GraphicsDevice.CreateMetal(options, swapChain);
//                 break;
//             case RendererType.Vulkan:
//                 Device = GraphicsDevice.CreateVulkan(options, swapChain);
//                 break;
//             case RendererType.Direct3D11:
//                 Device = GraphicsDevice.CreateD3D11(options, swapChain);
//                 break;
//             case RendererType.OpenGL:
//                 throw new NotSupportedException();
//             case RendererType.OpenGLLegacy:
//                 throw new NotSupportedException();
//             default:
//                 throw new ArgumentOutOfRangeException();
//         }
//
//
//         Logger.Verbose($"Created Veldrid device for {type}", Logger.RENDER);
//     }
// }