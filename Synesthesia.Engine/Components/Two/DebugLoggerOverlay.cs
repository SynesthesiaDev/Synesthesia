using System.Numerics;
using Common.Event;
using Common.Logger;
using Common.Util;
using Raylib_cs;
using Synesthesia.Engine.Configuration;
using Synesthesia.Engine.Dependency;
using Synesthesia.Engine.Graphics.Two.Drawables;
using Synesthesia.Engine.Graphics.Two.Drawables.Container;
using Synesthesia.Engine.Graphics.Two.Drawables.Text;
using Synesthesia.Engine.Threading.Runners;
using Synesthesia.Engine.Timing.Scheduling;
using Synesthesia.Engine.Utility;

namespace Synesthesia.Engine.Components.Two;

public class DebugLoggerOverlay : CompositeDrawable2d
{
    public const int MAX_MESSAGES = 10;
    public const int MAX_MESSAGE_LENGHT = 350;
    public const long MESSAGE_LIFESPAN = 5000;

    private FillFlowContainer2d _fillFlowContainer = null!;
    private EventSubscriber<Logger.LogEvent> _loggerSubscriber = null!;
    private Scheduler scheduler = null!;
    private Dictionary<Logger.LogEvent, DebugLoggerOverlayMessage> Messages = new();

    protected override void OnLoading()
    {
        AutoSizeAxes = Axes.Both;
        Children =
        [
            _fillFlowContainer = new FillFlowContainer2d
            {
                Direction = Direction.Vertical,
                AutoSizeAxes = Axes.Both,
            }
        ];

        scheduler = new Scheduler();
        _loggerSubscriber = Logger.MessageLogged.Subscribe(Push);
    }

    public void Push(Logger.LogEvent logEvent)
    {
        DependencyContainer.Get<UpdateThreadRunner>().Schedule(() =>
        {
            if (_fillFlowContainer.Children.Count() >= MAX_MESSAGES)
            {
                var last = _fillFlowContainer.Children.First();
                _fillFlowContainer.RemoveChild(last);
            }

            var message = new DebugLoggerOverlayMessage(logEvent);

            Messages.Add(logEvent, message);
            _fillFlowContainer.AddChild(message);

            scheduler.Schedule(MESSAGE_LIFESPAN, _ => Pop(logEvent));
        });
    }

    public void Pop(Logger.LogEvent logEvent)
    {
        if (Messages.Remove(logEvent, out var value))
        {
            _fillFlowContainer.RemoveChild(value);
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        Logger.MessageLogged.Unsubscribe(_loggerSubscriber);
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
                    BackgroundColor = Defaults.Background2,
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
                                    Prefix = $"{logEvent.severity.name} / {logEvent.type.name}",
                                    Color = ColorUtil.GetOrCacheColor(logEvent.severity.debugOverlayColor)
                                },
                                new TextDrawable
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = $"{logEvent.message.CutIfTooLong(MAX_MESSAGE_LENGHT)} "
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