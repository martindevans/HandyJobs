using Unity.Collections;
using Unity.Jobs;

namespace me.martindevans.handyjobs.Primitives.Collections
{
    public static class JobFillRandomExtensions
    {
        private const int DoubleBatchSize = 32;
        private const int SingleBatchSize = 64;

        #region double
        /// <summary>
        /// Schedule a job to fill an array with random data
        /// </summary>
        /// <param name="array"></param>
        /// <param name="seed"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle FillRandom(this NativeArray<double> array, uint seed, double min, double max, JobHandle inputDeps = default)
        {
            return array.Slice(0, array.Length).FillRandom(seed, min, max, inputDeps);
        }

        /// <summary>
        /// Schedule a job to fill a slice with random data
        /// </summary>
        /// <param name="array"></param>
        /// <param name="seed"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle FillRandom(this NativeSlice<double> array, uint seed, double min, double max, JobHandle inputDeps = default)
        {
            return new DoubleFillRandomJob(array, seed, min, max).Schedule(array.Length, DoubleBatchSize, inputDeps);
        }

        private struct DoubleFillRandomJob
            : IJobParallelForBatch
        {
            private NativeSlice<double> _dimensions;
            private readonly uint _seed;
            private readonly double _min;
            private readonly double _max;

            public DoubleFillRandomJob(NativeSlice<double> dimensions, uint seed, double min, double max)
            {
                _dimensions = dimensions;
                _seed = seed;
                _min = min;
                _max = max;
            }

            public void Execute(int startIndex, int count)
            {
                var rng = Unity.Mathematics.Random.CreateFromIndex(Seed(_seed, startIndex));

                for (var i = 0; i < count; i++)
                    _dimensions[startIndex + i] = rng.NextDouble(_min, _max);
            }
        }
        #endregion

        #region single
        /// <summary>
        /// Schedule a job to fill an array with random data
        /// </summary>
        /// <param name="array"></param>
        /// <param name="seed"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle FillRandom(this NativeArray<float> array, uint seed, float min, float max, JobHandle inputDeps = default)
        {
            return array.Slice(0, array.Length).FillRandom(seed, min, max, inputDeps);
        }

        /// <summary>
        /// Schedule a job to fill a slice with random data
        /// </summary>
        /// <param name="array"></param>
        /// <param name="seed"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle FillRandom(this NativeSlice<float> array, uint seed, float min, float max, JobHandle inputDeps = default)
        {
            return new SingleFillRandomJob(array, seed, min, max).Schedule(array.Length, SingleBatchSize, inputDeps);
        }

        private struct SingleFillRandomJob
            : IJobParallelForBatch
        {
            private NativeSlice<float> _dimensions;
            private readonly uint _seed;
            private readonly float _min;
            private readonly float _max;

            public SingleFillRandomJob(NativeSlice<float> dimensions, uint seed, float min, float max)
            {
                _dimensions = dimensions;
                _seed = seed;
                _min = min;
                _max = max;
            }

            public void Execute(int startIndex, int count)
            {
                var rng = Unity.Mathematics.Random.CreateFromIndex(Seed(_seed, startIndex));

                for (var i = 0; i < count; i++)
                    _dimensions[startIndex + i] = rng.NextFloat(_min, _max);
            }
        }
        #endregion

        #region int32
        /// <summary>
        /// Schedule a job to fill an array with random data
        /// </summary>
        /// <param name="array"></param>
        /// <param name="seed"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle FillRandom(this NativeArray<int> array, uint seed, int min, int max, JobHandle inputDeps = default)
        {
            return array.Slice(0, array.Length).FillRandom(seed, min, max, inputDeps);
        }

        /// <summary>
        /// Schedule a job to fill a slice with random data
        /// </summary>
        /// <param name="array"></param>
        /// <param name="seed"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public static JobHandle FillRandom(this NativeSlice<int> array, uint seed, int min, int max, JobHandle inputDeps = default)
        {
            return new Int32FillRandomJob(array, seed, min, max).Schedule(array.Length, SingleBatchSize, inputDeps);
        }

        private struct Int32FillRandomJob
            : IJobParallelForBatch
        {
            private NativeSlice<int> _dimensions;
            private readonly uint _seed;
            private readonly int _min;
            private readonly int _max;

            public Int32FillRandomJob(NativeSlice<int> dimensions, uint seed, int min, int max)
            {
                _dimensions = dimensions;
                _seed = seed;
                _min = min;
                _max = max;
            }

            public void Execute(int startIndex, int count)
            {
                var rng = Unity.Mathematics.Random.CreateFromIndex(Seed(_seed, startIndex));

                for (var i = 0; i < count; i++)
                    _dimensions[startIndex + i] = rng.NextInt(_min, _max);
            }
        }
        #endregion

        private static uint Seed(uint seed, int index)
        {
            return unchecked((uint)index + seed) & 0x7FFFFFFFu;
        }
    }
}
