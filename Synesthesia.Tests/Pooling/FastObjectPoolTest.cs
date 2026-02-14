// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Common.Pooling;

namespace Synesthesia.Tests.Pooling;

public class FastObjectPoolTest
{
    private class TestObject : IPooledObject
    {
        public bool IsReset { get; private set; }
        public void Reset() => IsReset = true;
        public bool IsPooled { get; set; }
        public Action<IPooledObject>? ReturnAction { get; set; }
        public void MarkUsed() => IsReset = false;
    }

    [Test]
    public void Rent_ShouldCreateNewInstance_WhenPoolIsEmpty()
    {
        var pool = new FastObjectPool<TestObject>(() => new TestObject());
        var item = pool.Rent();

        Assert.That(item, Is.Not.Null);
    }

    [Test]
    public void Return_ShouldReuseObject_OnSameThread()
    {
        var pool = new FastObjectPool<TestObject>(() => new TestObject());

        var first = pool.Rent();
        pool.Return(first); // should go to [ThreadStatic] _localItem

        var second = pool.Rent(); // gets from [ThreadStatic]

        Assert.That(first, Is.SameAs(second), "The pool should have returned the exact same instance.");
    }

    [Test]
    public void Return_ShouldResetObject_WhenImplemented()
    {
        var pool = new FastObjectPool<TestObject>(() => new TestObject());
        var item = pool.Rent();
        item.MarkUsed();

        pool.Return(item);

        Assert.That(item.IsReset, Is.True, "The object was not Reset() upon returning to the pool.");
    }

    [Test]
    public void ThreadStatic_ShouldProvideDifferentInstancesPerThread()
    {
        var pool = new FastObjectPool<TestObject>(() => new TestObject());
        TestObject thread1Item = null;
        TestObject thread2Item = null;

        var t1 = new Thread(() => {
            thread1Item = pool.Rent();
            pool.Return(thread1Item);
        });

        var t2 = new Thread(() => {
            thread2Item = pool.Rent();
            pool.Return(thread2Item);
        });

        t1.Start();
        t1.Join();
        t2.Start();
        t2.Join();

        Assert.That(thread1Item, Is.Not.SameAs(thread2Item), "Threads should not be sharing their local pool slots.");
    }

    [Test]
    public void SharedPool_ShouldHandleOverflow()
    {
        var pool = new FastObjectPool<TestObject>(() => new TestObject());

        var item1 = pool.Rent();
        var item2 = pool.Rent();

        pool.Return(item1);
        pool.Return(item2);

        var retrieved1 = pool.Rent();
        var retrieved2 = pool.Rent();

        Assert.That(retrieved1, Is.SameAs(item1));
        Assert.That(retrieved2, Is.SameAs(item2));
    }
}
