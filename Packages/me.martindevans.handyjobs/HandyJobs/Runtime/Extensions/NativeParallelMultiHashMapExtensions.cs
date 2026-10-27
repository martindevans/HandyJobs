using System;
using Unity.Collections;

namespace HandyJobs.Runtime.Extensions
{
    public static class NativeParallelMultiHashMapExtensions
    {
        public static NativeList<TV> GetValuesListForKey<TK, TV>(this NativeParallelMultiHashMap<TK, TV> map, TK key, Allocator allocator)
            where TK : unmanaged, IEquatable<TK>
            where TV : unmanaged
        {
            var output = new NativeList<TV>(32, allocator);
            foreach (var item in map.GetValuesForKey(key))
                output.Add(item);
            return output;
        }
    }
}
