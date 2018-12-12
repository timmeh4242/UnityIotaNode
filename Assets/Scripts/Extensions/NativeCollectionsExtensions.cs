using Unity.Entities;
using Unity.Collections;
using System;

public static partial class NativeCollectionsExtensions
{
    public static bool TryGetValues<TKey, TValue>(this NativeMultiHashMap<TKey, TValue> multiHashMap, TKey key, out NativeList<TValue> values, out NativeMultiHashMapIterator<TKey> iterator)
        where TKey : struct, IEquatable<TKey> where TValue : struct
    {
        values = new NativeList<TValue>(Allocator.Persistent);
        TValue value;
        if (!multiHashMap.TryGetFirstValue(key, out value, out iterator))
        {
            return false;
        }

        do
        {
            values.Add(value);
        } while (multiHashMap.TryGetNextValue(out value, ref iterator));
        return true;
    }
}
