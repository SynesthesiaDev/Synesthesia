using Synesthesia.Engine.Platform;
using Synesthesia.Engine.Platform.Host;

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
