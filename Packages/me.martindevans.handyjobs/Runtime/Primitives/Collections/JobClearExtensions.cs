using System;
using Unity.Collections;
using Unity.Jobs;

namespace me.martindevans.handyjobs.Primitives.Collections
{
    public static class JobClearExtensions
    {
        /// <summary>
        /// Schedule a job to clear a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Clear<T>(NativeList<T> list, JobHandle dependsOn)
            where T : unmanaged
        {
            return new ClearListJob<T>(list).Schedule(dependsOn);
        }

        private struct ClearListJob<T>
            : IJob
            where T : unmanaged
        {
            private NativeList<T> _list;

            public ClearListJob(NativeList<T> list)
            {
                _list = list;
            }

            public void Execute()
            {
                _list.Clear();
            }
        }


        /// <summary>
        /// Schedule a job to clear a set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Clear<T>(NativeHashSet<T> set, JobHandle dependsOn)
            where T : unmanaged, IEquatable<T>
        {
            return new ClearSetJob<T>(set).Schedule(dependsOn);
        }

        private struct ClearSetJob<T>
            : IJob
            where T : unmanaged, IEquatable<T>
        {
            private NativeHashSet<T> _set;

            public ClearSetJob(NativeHashSet<T> set)
            {
                _set = set;
            }

            public void Execute()
            {
                _set.Clear();
            }
        }


        /// <summary>
        /// Schedule a job to clear a queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Clear<T>(NativeQueue<T> list, JobHandle dependsOn)
            where T : unmanaged
        {
            return new ClearQueueJob<T>(list).Schedule(dependsOn);
        }

        private struct ClearQueueJob<T>
            : IJob
            where T : unmanaged
        {
            private NativeQueue<T> _queue;

            public ClearQueueJob(NativeQueue<T> queue)
            {
                _queue = queue;
            }

            public void Execute()
            {
                _queue.Clear();
            }
        }


        /// <summary>
        /// Schedule a job to clear a set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Clear<T>(NativeParallelHashSet<T> set, JobHandle dependsOn)
            where T : unmanaged, IEquatable<T>
        {
            return new ClearParallelSetJob<T>(set).Schedule(dependsOn);
        }

        private struct ClearParallelSetJob<T>
            : IJob
            where T : unmanaged, IEquatable<T>
        {
            private NativeParallelHashSet<T> _set;

            public ClearParallelSetJob(NativeParallelHashSet<T> set)
            {
                _set = set;
            }

            public void Execute()
            {
                _set.Clear();
            }
        }


        /// <summary>
        /// Schedule a job to clear a map
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="set"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Clear<K, V>(NativeHashMap<K, V> set, JobHandle dependsOn)
            where K : unmanaged, IEquatable<K>
            where V : unmanaged
        {
            return new ClearMapJob<K, V>(set).Schedule(dependsOn);
        }

        private struct ClearMapJob<K, V>
            : IJob
            where K : unmanaged, IEquatable<K>
            where V : unmanaged
        {
            private NativeHashMap<K, V> _map;

            public ClearMapJob(NativeHashMap<K, V> map)
            {
                _map = map;
            }

            public void Execute()
            {
                _map.Clear();
            }
        }


        /// <summary>
        /// Schedule a job to clear a map
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="set"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Clear<K, V>(NativeParallelHashMap<K, V> set, JobHandle dependsOn)
            where K : unmanaged, IEquatable<K>
            where V : unmanaged
        {
            return new ClearParallelMapJob<K, V>(set).Schedule(dependsOn);
        }

        private struct ClearParallelMapJob<K, V>
            : IJob
            where K : unmanaged, IEquatable<K>
            where V : unmanaged
        {
            private NativeParallelHashMap<K, V> _map;

            public ClearParallelMapJob(NativeParallelHashMap<K, V> map)
            {
                _map = map;
            }

            public void Execute()
            {
                _map.Clear();
            }
        }
    }
}
