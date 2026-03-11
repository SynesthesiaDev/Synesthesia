using Synesthesia.Engine.Platform;

namespace Synesthesia.Engine;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var window = new SDL3WindowHost();
        window.Initialize();
    }
}
