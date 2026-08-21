using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace me.martindevans.handyjobs.Primitives.Collections
{
    public static class JobReadIndicesExtensions
    {
        /// <summary>
        /// Read items specified in the indices array from the source array and append them to the output list
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <param name="source0">Source to read items from</param>
        /// <param name="indices">Indices to copy from source to output</param>
        /// <param name="output">List to append items to</param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle ReadIndices<T0>(this NativeArray<int> indices, NativeArray<T0> source0, NativeList<T0> output, JobHandle dependsOn = default)
            where T0 : unmanaged
        {
            return new ReadIndicesJob1<T0>(indices, source0, output).Schedule(dependsOn);
        }

        /// <summary>
        /// Given an array of indices, extract items by index from another array and add the results to an output list
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        [BurstCompile]
        private struct ReadIndicesJob1<T1>
            : IJob
            where T1 : unmanaged
        {
            [ReadOnly] private readonly NativeArray<int> _indices;
            [ReadOnly] private readonly NativeArray<T1> _source;

            private NativeList<T1> _results;

            public ReadIndicesJob1(NativeArray<int> indices, NativeArray<T1> source, NativeList<T1> results)
            {
                _indices = indices;
                _source = source;
                _results = results;
            }

            public void Execute()
            {
                foreach (var idx in _indices)
                    _results.Add(_source[idx]);
            }
        }

        /// <summary>
        /// Read items specified in the indices array from the source array and append them to the output list
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="indices">Indices to copy from source to output</param>
        /// <param name="source0">Source to read items from</param>
        /// <param name="source1">Source to read items from</param>
        /// <param name="output">List to append items to</param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle ReadIndices<T0, T1>(this NativeArray<(int, int)> indices, NativeArray<T0> source0, NativeArray<T1> source1, NativeList<(T0, T1)> output, JobHandle dependsOn = default)
            where T0 : unmanaged
            where T1 : unmanaged

        {
            return new ReadIndicesJob2<T0, T1>(
                indices,
                source0,
                source1,
                output
            ).Schedule(dependsOn);
        }

        /// <summary>
        /// Given an array of pairs of indices, extract items by index from 2 other arrays and add the results to an output list
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// Warn: This cannot be Burst compiled due to use of tuples!
        private struct ReadIndicesJob2<T0, T1>
            : IJob
            where T0 : unmanaged
            where T1 : unmanaged
        {
            [ReadOnly] private readonly NativeArray<(int, int)> _indices;
            [ReadOnly] private readonly NativeArray<T0> _src0;
            [ReadOnly] private readonly NativeArray<T1> _src2;

            private NativeList<(T0, T1)> _results;

            public ReadIndicesJob2(NativeArray<(int, int)> indices, NativeArray<T0> src0, NativeArray<T1> src2, NativeList<(T0, T1)> results)
            {
                _indices = indices;
                _src0 = src0;
                _src2 = src2;
                _results = results;
            }

            public void Execute()
            {
                foreach (var (i0, i1) in _indices)
                    _results.Add((_src0[i0], _src2[i1]));
            }
        }
    }
}
