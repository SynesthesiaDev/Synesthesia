// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;
using Common.Statistics;

namespace Common.Pooling;

// ReSharper disable InvertIf
public class FastObjectPool<T>(Func<T> activator) : IDisposable where T : class
{
    private readonly T?[] sharedItems = new T?[32];

    private bool isDisposed;

    // [ThreadStatic]
    // private static T? localItem;

    [ThreadStatic]
    private static (T item, FastObjectPool<T> owner)? localItem;

    public void PreAllocate(int count)
    {
        lock (sharedItems)
        {
            int allocated = 0;
            for (int i = 0; i < sharedItems.Length && allocated < count; i++)
            {
                if (sharedItems[i] == null)
                {
                    sharedItems[i] = activator.Invoke();
                    allocated++;
                }
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

        EngineStatistics.OBJECTS_RENTED.Increment();
        EngineStatistics.OBJECTS_ALIVE.Increment();
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

        EngineStatistics.OBJECTS_RETURNED.Increment();
        EngineStatistics.OBJECTS_ALIVE.Decrement();
    }

    private T rentFromShared()
    {
        lock (sharedItems)
        {
            for (var i = 0; i < sharedItems.Length; i++)
            {
                var instance = sharedItems[i];
                if (instance != null)
                {
                    sharedItems[i] = null;
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
            for (int i = 0; i < sharedItems.Length; i++)
            {
                if (sharedItems[i] == null)
                {
                    sharedItems[i] = item;
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
            for (int i = 0; i < sharedItems.Length; i++)
            {
                var item = sharedItems[i];
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                sharedItems[i] = null;
            }
        }
    }
}
