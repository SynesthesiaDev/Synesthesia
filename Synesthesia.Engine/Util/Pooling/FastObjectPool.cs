// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;
using Synesthesia.Engine.Extensions;
using Synesthesia.Engine.Util.Statistics;

namespace Synesthesia.Engine.Util.Pooling;

// ReSharper disable InvertIf
public class FastObjectPool<T>(Func<T> activator, int capacity = 32) : IDisposable where T : class
{
    private readonly T?[] sharedItems = new T?[capacity];

    private bool isDisposed;

    [ThreadStatic]
    private static (T item, FastObjectPool<T> owner)? localItem;

    public void PreAllocate(int count)
    {
        lock (sharedItems)
        {
            var index = 0;
            foreach (var sharedItem in sharedItems)
            {
                if (sharedItem == null)
                {
                    sharedItems[index] = activator.Invoke();
                }
                index++;
            }
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Rent()
    {
        var item = rentInternal();
        if (item is IPooledObject pooledObject)
        {
            pooledObject.IsPooled = true;
            pooledObject.ReturnAction = obj => Return((T)obj);
        }

        EngineStatistics.Increment(EngineStatistics.Type.ObjectsRented);
        EngineStatistics.Increment(EngineStatistics.Type.ObjectsInflight);
        return item;
    }

    private T rentInternal()
    {
        if (localItem.HasValue && localItem.Value.owner == this)
        {
            var item = localItem.Value.item;
            localItem = null;
            return item;
        }
        return rentFromShared();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return(T item)
    {
        if (item is IPooledObject pooled) pooled.Reset();

        if (localItem == null)
        {
            localItem = new ValueTuple<T, FastObjectPool<T>>(item, this);
        }
        else
        {
            returnToShared(item);
        }

        EngineStatistics.Increment(EngineStatistics.Type.ObjectsReturned);
        EngineStatistics.Decrement(EngineStatistics.Type.ObjectsInflight);
    }

    private T rentFromShared()
    {
        lock (sharedItems)
        {
            foreach (var (index, instance) in sharedItems.WithIndex())
            {
                if (instance != null)
                {
                    sharedItems[index] = null;
                    return instance;
                }
            }
        }

        return activator.Invoke();
    }

    private void returnToShared(T item)
    {
        lock (sharedItems)
        {
            foreach (var (index, instance) in sharedItems.WithIndex())
            {
                if (instance == null)
                {
                    sharedItems[index] = item;
                    return;
                }
            }
        }
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        localItem = null;

        lock (sharedItems)
        {
            foreach (var (index, instance) in sharedItems.WithIndex())
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                sharedItems[index] = null;
            }
        }
    }
}
