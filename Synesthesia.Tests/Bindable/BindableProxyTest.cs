
using System.Numerics;
using Common.Bindable;
using Common.Event;
using Raylib_cs;

namespace Synesthesia.Tests.Bindable;

public class Tests
{

    protected EventDispatcher<Vector2> OnMouseMove = new();
    protected EventDispatcher<KeyboardKey> OnKeyDown = new();

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var proxy = new BindableProxy();
        var latch = new CountdownEvent(2);

        proxy.Subscribe(OnMouseMove, _ => latch.Signal());
        proxy.Subscribe(OnKeyDown, _ => latch.Signal());

        OnMouseMove.Dispatch(Vector2.Zero);
        OnKeyDown.Dispatch(KeyboardKey.A);

        latch.Wait(10);

        Assert.That(latch.CurrentCount, Is.EqualTo(0));

        latch.Reset();
        proxy.Dispose();

        OnMouseMove.Dispatch(Vector2.Zero);
        OnKeyDown.Dispatch(KeyboardKey.A);

        latch.Wait(10);
        Assert.That(latch.CurrentCount, Is.EqualTo(2));
    }
}
