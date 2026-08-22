using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Packages.HandyJobs.Runtime.Primitives.Collections
{
    public static class JobSetCapacityExtensions
    {
        #region NativeParallelHashSet
        /// <summary>
        /// Schedule a job that will set the capacity of the hashset to the length of the list.
        /// </summary>
        /// <remarks>This will never shrink capacity! if capacity is already larger than list length it will do nothing.</remarks>
        /// <typeparam name="TSetItem"></typeparam>
        /// <typeparam name="TList"></typeparam>
        /// <typeparam name="TListItem"></typeparam>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="factor">Capacity will be set to list.Length * factor</param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle SetCapacity<TSetItem, TList, TListItem>(this NativeParallelHashSet<TSetItem> dst, TList src, float factor, JobHandle dependsOn)
            where TSetItem : unmanaged, IEquatable<TSetItem>
            where TList : INativeList<TListItem>
            where TListItem : unmanaged
        {
            return new SetNativeParallelHashSetCapacityFromList<TSetItem, TList, TListItem>(dst, src, factor).Schedule(dependsOn);
        }

        /// <summary>
        /// Schedule a job that will set the capacity of the hashset to the length of the list.
        /// </summary>
        /// <remarks>This will never shrink capacity! if capacity is already larger than list length it will do nothing.</remarks>
        /// <typeparam name="TSetItem"></typeparam>
        /// <typeparam name="TListItem"></typeparam>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="factor">Capacity will be set to list.Length * factor</param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle SetCapacity<TSetItem, TListItem>(this NativeParallelHashSet<TSetItem> dst, NativeList<TListItem> src, float factor, JobHandle dependsOn)
            where TSetItem : unmanaged, IEquatable<TSetItem>
            where TListItem : unmanaged
        {
            return dst.SetCapacity<TSetItem, NativeList<TListItem> , TListItem>(src, factor, dependsOn);
        }

        /// <summary>
        /// Schedule a job that will set the capaxity of the hashset to the length of the list.
        /// </summary>
        /// <remarks>This will never shrink capacity! if capacity is already larger than list length it will do nothing.</remarks>
        /// <typeparam name="TSetItem"></typeparam>
        /// <typeparam name="TList"></typeparam>
        /// <typeparam name="TListItem"></typeparam>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle SetCapacity<TSetItem, TList, TListItem>(this NativeParallelHashSet<TSetItem> dst, TList src, JobHandle dependsOn)
            where TSetItem : unmanaged, IEquatable<TSetItem>
            where TList : INativeList<TListItem>
            where TListItem : unmanaged
        {
            return dst.SetCapacity<TSetItem, TList, TListItem>(src, factor: 1, dependsOn);
        }

        /// <summary>
        /// Schedule a job that will set the capacity of the hashset to the length of the list.
        /// </summary>
        /// <remarks>This will never shrink capacity! if capacity is already larger than list length it will do nothing.</remarks>
        /// <typeparam name="TSetItem"></typeparam>
        /// <typeparam name="TListItem"></typeparam>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle SetCapacity<TSetItem, TListItem>(this NativeParallelHashSet<TSetItem> dst, NativeList<TListItem> src, JobHandle dependsOn)
            where TSetItem : unmanaged, IEquatable<TSetItem>
            where TListItem : unmanaged
        {
            return dst.SetCapacity<TSetItem, NativeList<TListItem>, TListItem>(src, dependsOn);
        }

        private struct SetNativeParallelHashSetCapacityFromList<TSetItem, TList, TListItem>
            : IJob
            where TSetItem : unmanaged, IEquatable<TSetItem>
            where TList : INativeList<TListItem>
            where TListItem : unmanaged
        {
            private NativeParallelHashSet<TSetItem> _set;
            private readonly TList _list;
            private readonly float _factor;

            public SetNativeParallelHashSetCapacityFromList(NativeParallelHashSet<TSetItem> set, TList list, float factor)
            {
                _set = set;
                _list = list;
                _factor = factor;
            }

            public void Execute()
            {
                _set.Capacity = math.max(
                    (int)math.ceil(_list.Length * _factor),
                    _set.Capacity
                );
            }
        }
        #endregion
    }
}
