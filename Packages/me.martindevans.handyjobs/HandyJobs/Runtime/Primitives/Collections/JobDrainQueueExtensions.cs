using Packages.HandyJobs.Runtime.Extensions;
using Unity.Collections;
using Unity.Jobs;

namespace Packages.HandyJobs.Runtime.Primitives.Collections
{
    public static class JobDrainQueueExtensions
    {
        /// <summary>
        /// Schedule a job to drain all of the elements of a queue and append them to a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <param name="list"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle DrainTo<T>(this NativeQueue<T> queue, NativeList<T> list, JobHandle dependsOn)
            where T : unmanaged
        {
            return new DrainQueueToListJob<T>(queue, list).Schedule(dependsOn);
        }

        private struct DrainQueueToListJob<T>
            : IJob
            where T : unmanaged
        {
            private NativeQueue<T> _queue;
            private NativeList<T> _list;

            public DrainQueueToListJob(NativeQueue<T> queue, NativeList<T> list)
            {
                _queue = queue;
                _list = list;
            }

            public void Execute()
            {
                _list.EnsureCapacity(_list.Length + _queue.Count);

                while (_queue.TryDequeue(out var item))
                    _list.Add(item);
            }
        }
    }
}
