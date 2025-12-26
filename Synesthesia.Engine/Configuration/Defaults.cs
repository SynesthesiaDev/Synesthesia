using Raylib_cs;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Configuration;

public static class Defaults
{
    public const int RendererRate = 240;
    public const int UpdateRate = RendererRate * 2;
    public const int InputRate = 1000;
    public const int AudioRate = 1000;

    public static readonly Color Primary = new(147, 238, 255);
    public static readonly Color Secondary = new(56, 154, 172);
    public static readonly Color Accent = new(77, 207, 230);
    public static readonly Color Text = new(231, 242, 245);

    public static readonly Color Background0 = ColorUtil.FromHex("#0B0B0B");
    public static readonly Color Background1 = ColorUtil.FromHex("#151515");
    public static readonly Color Background2 = ColorUtil.FromHex("#2A2A2A");
    public static readonly Color Background3 = ColorUtil.FromHex("#515151");
    public static readonly Color Background4 = ColorUtil.FromHex("#7B7B7B");
    public static readonly Color Background5 = ColorUtil.FromHex("#9E9E9E");
}