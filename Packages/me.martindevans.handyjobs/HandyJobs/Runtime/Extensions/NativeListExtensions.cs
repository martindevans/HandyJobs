using Unity.Collections;

namespace Packages.HandyJobs.Runtime.Extensions
{
    public static class NativeListExtensions
    {
        public static void EnsureCapacity<T>(this NativeList<T> list, int capacity)
            where T : unmanaged
        {
            if (capacity > list.Capacity)
                list.SetCapacity(capacity);
        }
    }
}
