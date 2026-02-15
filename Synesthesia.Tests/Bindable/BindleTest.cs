// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Bindable;

namespace Synesthesia.Tests.Bindable;

public class BindleTest
{
    [Test]
    public void TestBindable()
    {
        var bindable = new Bindable<string>(string.Empty);
        var newPropagatedValue = string.Empty;
        var oldPropagatedValue = string.Empty;
        var signal = new CountdownEvent(1);

        Assert.That(bindable.Value, Is.EqualTo(string.Empty));
        Assert.That(bindable.Bound, Is.Null);

        var listener = bindable.OnValueChange(e =>
        {
            newPropagatedValue = e.NewValue;
            oldPropagatedValue = e.OldValue;

            signal.Signal();
        });

        bindable.Value = "test";
        signal.Wait();

        Assert.That(bindable.Value, Is.EqualTo("test"));
        Assert.That(newPropagatedValue, Is.EqualTo("test"));
        Assert.That(oldPropagatedValue, Is.EqualTo(string.Empty));

        signal.Reset();
        bindable.Unregister(listener);

        bindable.Value = "yippee";

        signal.Wait(50);

        Assert.That(bindable.Value, Is.EqualTo("yippee"));

        // make sure did not change
        Assert.That(newPropagatedValue, Is.EqualTo("test"));
        Assert.That(oldPropagatedValue, Is.EqualTo(string.Empty));

        bindable.Dispose();
        signal.Dispose();
    }

    [Test]
    public void TestBinding()
    {
        var bindable1 = new Bindable<string>(string.Empty);
        var bindable2 = new Bindable<string>(string.Empty);
        var signal = new CountdownEvent(1);

        Assert.Throws<InvalidOperationException>(() => bindable1.BindTo(bindable1));

        bindable2.BindTo(bindable1);

        Assert.That(bindable2.Bound, !Is.Null);
        Assert.That(bindable1.Bound, Is.Null);

        bindable2.OnValueChange(_ => signal.Signal());
        bindable1.Value = "yippee";

        signal.Wait();

        Assert.That(bindable1.Value, Is.EqualTo("yippee"));
        Assert.That(bindable2.Value, Is.EqualTo("yippee"));

        Assert.Throws<InvalidOperationException>(() => bindable2.BindTo(bindable1));

        signal.Reset();
        bindable2.Unbind();

        bindable1.Value = "testing";
        signal.Wait(50);

        Assert.That(bindable1.Value, Is.EqualTo("testing"));
        Assert.That(bindable2.Value, Is.EqualTo("yippee"));

        bindable2.Dispose();
        bindable1.Dispose();
        signal.Dispose();
    }

    [Test]
    public void TestEventSource()
    {
        var eventSource = new BindableEventSource();
        var bindable = new Bindable<string>(string.Empty);
        var signal = new CountdownEvent(1);
        var propagatedValue = string.Empty;

        bindable.OnValueChange(e =>
        {
            propagatedValue = e.NewValue;
            signal.Signal();
        }, ignoresSource: eventSource);

        bindable.Set("yippee", eventSource);

        signal.Wait(50);

        Assert.That(propagatedValue, Is.EqualTo(string.Empty));
        Assert.That(signal.CurrentCount, Is.EqualTo(signal.InitialCount));

        bindable.Value = "testing";

        signal.Wait(50);

        Assert.That(propagatedValue, Is.EqualTo("testing"));

        bindable.Dispose();
        signal.Dispose();
    }

    public void TestBindableFloat()
    {
        var bindable = new BindableFloat
        {
            Max = 100,
            Min = 50
        };

        var signal = new CountdownEvent(1);
        var propagatedValue = 0f;

        bindable.OnValueChange(e =>
        {
            propagatedValue = e.NewValue;
            signal.Signal();
        });


        bindable.Value = 67f;

        signal.Wait();

        Assert.That(bindable.Value, Is.EqualTo(67f));
        Assert.That(propagatedValue, Is.EqualTo(67f));

        signal.Reset();

        bindable.Value = 585885;

        signal.Wait();

        Assert.That(bindable.Value, Is.EqualTo(bindable.Max));
        Assert.That(propagatedValue, Is.EqualTo(bindable.Max));

        signal.Reset();

        bindable.Value = 23;

        signal.Wait();

        Assert.That(bindable.Value, Is.EqualTo(bindable.Min));
        Assert.That(propagatedValue, Is.EqualTo(bindable.Min));

        bindable.Dispose();
        signal.Dispose();
    }
}
