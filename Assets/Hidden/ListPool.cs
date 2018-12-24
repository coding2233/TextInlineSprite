using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//对象池 -- Unity-Technologies-UI
public static class ListPool<T>
{
    // Object pool to avoid allocations.
    private static readonly ObjectPool<List<T>> _listPool = new ObjectPool<List<T>>(null, Clear);
    static void Clear(List<T> l) { l.Clear(); }

    public static List<T> Get()
    {
        return _listPool.Get();
    }

    public static void Release(List<T> toRelease)
    {
        _listPool.Release(toRelease);
    }
}