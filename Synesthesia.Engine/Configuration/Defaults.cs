using Raylib_cs;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Configuration;

public static class Defaults
{
    public const int RENDERER_RATE = 240;
    public const int UPDATE_RATE = RENDERER_RATE * 2;
    public const int INPUT_RATE = 1000;
    public const int AUDIO_RATE = 1000;

    public static readonly Color PRIMARY = new(147, 238, 255);
    public static readonly Color SECONDARY = new(56, 154, 172);
    public static readonly Color ACCENT = new(77, 207, 230);
    public static readonly Color TEXT = new(231, 242, 245);

    public static readonly Color BACKGROUND0 = ColorUtil.FromHex("#0B0B0B");
    public static readonly Color BACKGROUND1 = ColorUtil.FromHex("#151515");
    public static readonly Color BACKGROUND2 = ColorUtil.FromHex("#2A2A2A");
    public static readonly Color BACKGROUND3 = ColorUtil.FromHex("#515151");
    public static readonly Color BACKGROUND4 = ColorUtil.FromHex("#7B7B7B");
    public static readonly Color BACKGROUND5 = ColorUtil.FromHex("#9E9E9E");
}