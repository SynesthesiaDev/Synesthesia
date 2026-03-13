// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;
using Synesthesia.Engine.Future;
using Synesthesia.Engine.Logging;

namespace Synesthesia.Engine.Input;

public sealed class TabletDriver : Driver
{
    private readonly int[] knownVendors;

    private static readonly string[] ignored_logs =
    [
        "Changes: ",
        "Searching for tablet",
        "Invoking DevicesChanged",
    ];

    public event EventHandler<IDeviceReport>? DeviceReported;

    public TabletDriver(ICompositeDeviceHub deviceHub, IReportParserProvider reportParserProvider, IDeviceConfigurationProvider configurationProvider)
        : base(deviceHub, reportParserProvider, configurationProvider)
    {
        var vendors = from config in configurationProvider.TabletConfigurations
            from id in config.DigitizerIdentifiers
            select id.VendorID;

        knownVendors = vendors.Distinct().ToArray();

        Log.Output += driverLog;

        deviceHub.DevicesChanged += (_, args) =>
        {
            // it's worth noting that this event fires on *any* device change system-wide, including non-tablet devices.
            if (!Tablets.Any() && args.Additions.Any())
                detectDevicesAsync();
        };

        detectDevicesAsync();
    }

    private void driverLog(object? _, LogMessage logMessage)
    {
        if (ignored_logs.Any(ignoredLog => logMessage.Message.StartsWith(ignoredLog, StringComparison.Ordinal))) return;

        switch (logMessage.Level)
        {
            case LogLevel.Debug:
                Logger.Verbose($"{logMessage.Message}", Logger.Input);
                break;
            case LogLevel.Info:
                Logger.Debug($"{logMessage.Message}", Logger.Input);
                break;
            case LogLevel.Warning:
                Logger.Warning($"{logMessage.Message}", Logger.Input);
                break;
            case LogLevel.Error:
                Logger.Error($"{logMessage.Message}", Logger.Input);
                break;
            case LogLevel.Fatal:
                Logger.Error($"{logMessage.Message}: {logMessage.StackTrace}", Logger.Input);
                break;
        }
    }

    private void detectDevicesAsync()
    {
        CompletableFuture.RunAsync(() =>
        {
            Thread.Sleep(50);
            int vendor = CompositeDeviceHub.GetDevices().Select(d => d.VendorID).Intersect(knownVendors).FirstOrDefault();
            if (vendor <= 0) return;

            Logger.Verbose($"Tablet detected (vid{vendor}), searching for usable configuration...", Logger.Input);
            Detect();

            foreach (var endpoint in InputDevices.SelectMany(device => device.InputDevices))
            {
                endpoint.Report += DeviceReported;
                endpoint.ConnectionStateChanged += (_, connected) =>
                {
                    if (!connected)
                        endpoint.Report -= DeviceReported;
                };
            }
        });
    }

    public static TabletDriver Create()
    {
        IServiceCollection serviceCollection = new DriverServiceCollection()
            .AddTransient<TabletDriver>();

        var provider = serviceCollection.BuildServiceProvider();

        return provider.GetRequiredService<TabletDriver>();
    }

    public new void Dispose()
    {
        base.Dispose();
        Log.Output -= driverLog;
    }
}
