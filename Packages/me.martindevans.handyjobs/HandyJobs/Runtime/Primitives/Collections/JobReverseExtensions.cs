using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Packages.HandyJobs.Runtime.Primitives.Collections
{
    public static class JobReverseExtensions
    {
        /// <summary>
        /// Schedule a job to in-place reverse a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Reverse<T>(this NativeList<T> list, JobHandle dependsOn)
            where T : unmanaged
        {
            return new ReverseJob<T>(list.AsDeferredJobArray()).Schedule(dependsOn);
        }

        /// <summary>
        /// Schedule a job to in-place reverse an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle Reverse<T>(this NativeArray<T> array, JobHandle dependsOn)
            where T : unmanaged
        {
            return new ReverseJob<T>(array).Schedule(dependsOn);
        }

        [BurstCompile]
        private readonly struct ReverseJob<T>
            : IJob
            where T : unmanaged
        {
            private readonly NativeArray<T> _array;

            public ReverseJob(NativeArray<T> array)
            {
                _array = array;
            }

            public void Execute()
            {
                _array.AsSpan().Reverse();
            }
        }
    }
}
