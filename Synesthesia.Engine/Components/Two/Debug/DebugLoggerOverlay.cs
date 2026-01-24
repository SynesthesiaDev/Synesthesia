using System.Numerics;
using Common.Event;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Animations.Easings;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Threading.Runners;
using Synesthesia.Engine.Timing.Scheduling;
using Synesthesia.Engine.Utility;
using SynesthesiaUtil.Extensions;

namespace Synesthesia.Engine.Components.Two.Debug;

public class DebugLoggerOverlay : CompositeDrawable2d
{
    public const int MAX_MESSAGES = 10;
    public const int MAX_MESSAGE_LENGHT = 350;
    public const long MESSAGE_LIFESPAN = 5000;

    private FillFlowContainer2d fillFlowContainer = null!;
    private EventSubscriber<Logger.LogEvent> loggerSubscriber = null!;
    private Scheduler scheduler = null!;
    private Dictionary<Logger.LogEvent, DebugLoggerOverlayMessage> messages = new();

    protected override void OnLoading()
    {
        AutoSizeAxes = Axes.Both;
        Children =
        [
            fillFlowContainer = new FillFlowContainer2d
            {
                Direction = Direction.Vertical,
                AutoSizeAxes = Axes.Both,
            }
        ];

        scheduler = new Scheduler();
        loggerSubscriber = Logger.MESSAGE_LOGGED.Subscribe(Push);
    }

    public void Push(Logger.LogEvent logEvent)
    {
        DependencyContainer.Get<UpdateThreadRunner>().Schedule(() =>
        {
            if (fillFlowContainer.Children.Count() >= MAX_MESSAGES)
            {
                var last = fillFlowContainer.Children.First();
                fillFlowContainer.RemoveChild(last);
            }

            var message = new DebugLoggerOverlayMessage(logEvent);

            messages.Add(logEvent, message);
            fillFlowContainer.AddChild(message);

            scheduler.Schedule(MESSAGE_LIFESPAN, _ => Pop(logEvent));
        });
    }

    public void Pop(Logger.LogEvent logEvent)
    {
        if (messages.Remove(logEvent, out var value))
        {
            value.FadeTo(0f, 500, Easing.Out).Then(() => fillFlowContainer.RemoveChild(value));
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        Logger.MESSAGE_LOGGED.Unsubscribe(loggerSubscriber);
        scheduler.Dispose();
        base.Dispose(isDisposing);
    }

    private class DebugLoggerOverlayMessage(Logger.LogEvent logEvent) : CompositeDrawable2d
    {
        protected override void OnLoading()
        {
            AutoSizeAxes = Axes.Both;
            Children =
            [
                new BackgroundContainer2d
                {
                    AutoSizeAxes = Axes.Both,
                    BackgroundColor = Defaults.BACKGROUND2,
                    AutoSizePadding = new Vector4(4, 4, 4, 4),
                    BackgroundCornerRadius = 10,
                    BackgroundAlpha = 0.8f,
                    Children =
                    [
                        new FillFlowContainer2d
                        {
                            Direction = Direction.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = 4,
                            Children =
                            [
                                new MessagePrefix
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Prefix = $"{logEvent.Severity.Name} / {logEvent.Category.Name}",
                                    Color = ColorUtil.GetOrCacheColor(logEvent.Severity.DebugOverlayColor)
                                },
                                new TextDrawable
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = $"{logEvent.Message.CutIfTooLong(MAX_MESSAGE_LENGHT, true)} "
                                }
                            ]
                        }
                    ]
                },
            ];
        }
    }

    private class MessagePrefix : CompositeDrawable2d
    {
        public string Prefix { get; set; } = "Prefix";

        public Color Color { get; set; } = Color.Black;

        protected override void OnLoading()
        {
            AutoSizeAxes = Axes.Both;
            Children =
            [
                new BackgroundContainer2d
                {
                    AutoSizeAxes = Axes.Both,
                    AutoSizePadding = new Vector4(1),
                    BackgroundColor = Color,
                    BackgroundCornerRadius = 10,
                    Children =
                    [
                        new TextDrawable
                        {
                            Text = $" {Prefix} ",
                            Color = Color.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    ]
                }
            ];
        }
    }
}