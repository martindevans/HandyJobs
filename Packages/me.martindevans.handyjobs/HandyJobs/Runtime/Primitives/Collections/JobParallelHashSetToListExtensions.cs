using System;
using Unity.Collections;
using Unity.Jobs;

namespace Packages.HandyJobs.Runtime.Primitives.Collections
{
    public static class JobParallelHashSetToListExtensions
    {
        /// <summary>
        /// Copy all of the items from a parallel hash set to a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle CopyToList<T>(this NativeParallelHashSet<T> input, NativeList<T> output, JobHandle dependsOn)
            where T : unmanaged, IEquatable<T>
        {
            return new CopySetToListJob<T>(input, output)
               .Schedule(dependsOn);
        }

        private struct CopySetToListJob<T>
            : IJob
            where T : unmanaged, IEquatable<T>
        {
            private readonly NativeParallelHashSet<T> _input;
            private NativeList<T> _output;

            public CopySetToListJob(NativeParallelHashSet<T> input, NativeList<T> output)
            {
                _input = input;
                _output = output;
            }

            public void Execute()
            {
                foreach (var item in _input)
                    _output.Add(item);
            }
        }
    }
}
