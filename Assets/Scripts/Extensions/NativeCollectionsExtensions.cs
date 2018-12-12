using Unity.Entities;
using Unity.Collections;

public static partial class NativeCollectionsExtensions
{
    public static bool TryGetValues(this NativeMultiHashMap<Entity, Entity> multiHashMap, Entity root, out NativeList<Entity> values, out NativeMultiHashMapIterator<Entity> iterator)
    {
        values = new NativeList<Entity>(Allocator.Persistent);
        Entity child;
        if (!multiHashMap.TryGetFirstValue(root, out child, out iterator))
        {
            values.Add(root);
            return false;
        }

        do
        {
            values.Add(child);
        } while (multiHashMap.TryGetNextValue(out child, ref iterator));

        return true;
    }
}
