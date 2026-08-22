using System;
using Unity.Collections;
using Unity.Jobs;

namespace Packages.HandyJobs.Runtime.Primitives.Collections
{
    public static class JobReadIndexExtensions
    {
        /// <summary>
        /// Schedule a job to read an indexed element from an array. If the index is out of range, nothing is written to the output.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="index"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle ReadIndex<T>(this NativeArray<T> input, NativeReference<T> output, Index index, JobHandle inputDeps = default)
            where T : unmanaged
        {
            return new ReadIndexJob<T>(index, input, output).Schedule(inputDeps);
        }

        /// <summary>
        /// Schedule a job to read an indexed element from a list. If the index is out of range, nothing is written to the output.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="index"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle ReadIndex<T>(this NativeList<T> input, NativeReference<T> output, Index index, JobHandle inputDeps = default)
            where T : unmanaged
        {
            return new ReadIndexJob<T>(index, input.AsDeferredJobArray(), output).Schedule(inputDeps);
        }

        private struct ReadIndexJob<T>
            : IJob
            where T : unmanaged
        {
            private readonly Index _index;
            private readonly NativeArray<T> _input;
            private NativeReference<T> _output;

            public ReadIndexJob(Index index, NativeArray<T> input, NativeReference<T> output)
            {
                _index = index;
                _input = input;
                _output = output;
            }

            public void Execute()
            {
                var idx = _index.GetOffset(_input.Length);

                // Write nothing if the index is out of range
                if (idx < 0 || idx >= _input.Length)
                    return;

                _output.Value = _input[idx];
            }
        }
    }
}
