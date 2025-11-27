using Common.Logger;
using Common.Util;
using NUnit.Framework;

namespace Synesthesia.Tests;

public class Logging
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        if (!EnvUtils.IsRunningInTestEnvironment()) return;

        Pastel.ConsoleExtensions.Disable();
        Logger.Verbose("Disabled colored log messages in test environment");
    }

    [SetUp]
    public void Setup()
    {
        Logger.LogCount.Value = 0;
        Logger.Enabled = true;
    }

    [Test]
    public void PrintAll()
    {
        Logger.Verbose("Verbose");
        Logger.Debug("Debug");
        Logger.Warning("Warning");
        Logger.Error("Warning");
    }

    [Test]
    public void TestLoggingEnabled()
    {
        Assert.That(Logger.Enabled);
        Assert.That(Logger.LogCount.Value == 0);

        Logger.Debug("test");

        Assert.That(Logger.LogCount.Value == 1);

        Logger.Enabled = false;

        Logger.Debug("test");

        Assert.That(!Logger.Enabled);
        Assert.That(Logger.LogCount.Value == 1);
    }
}