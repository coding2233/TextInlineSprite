using System.Collections;
using System.Collections.Generic;


public static class Pool<T> where T:new()  {

    private static readonly ObjectPool<T> _objectPool = new ObjectPool<T>(null, null);

    public static T Get()
    {
        return _objectPool.Get();
    }

    public static void Release(T element)
    {
        _objectPool.Release(element);
    }


}
