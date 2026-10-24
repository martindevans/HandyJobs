using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace HandyJobs.Runtime.Primitives.Collections
{
    [BurstCompile]
    public static class JobNativeHashSetCopyToListExtensions
    {
        /// <summary>
        /// Copy the items from a set to a list in a job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="output"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle CopyToList<T>(this NativeHashSet<T> set, NativeList<T> output, JobHandle inputDeps = default)
            where T : unmanaged, IEquatable<T>
        {
            return new CopyToListJob<T>(set, output).Schedule(inputDeps);
        }

        [BurstCompile]
        private struct CopyToListJob<T>
            : IJob
            where T : unmanaged, IEquatable<T>
        {
            private readonly NativeHashSet<T> _set;
            private NativeList<T> _output;

            public CopyToListJob(NativeHashSet<T> set, NativeList<T> output)
            {
                _set = set;
                _output = output;
            }

            public void Execute()
            {
                foreach (var item in _set)
                {
                    _output.Add(item);
                }
            }
        }
    }
}
