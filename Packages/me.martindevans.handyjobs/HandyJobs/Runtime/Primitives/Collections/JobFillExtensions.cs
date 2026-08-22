using Packages.HandyJobs.Runtime.Extensions;
using Unity.Collections;
using Unity.Jobs;

namespace Packages.HandyJobs.Runtime.Primitives.Collections
{
    public static class JobFillExtensions
    {
        /// <summary>
        /// Schedule a job that will fill the given array with one value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="inputDeps"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JobHandle Fill<T>(this NativeArray<T> array, JobHandle inputDeps, T value = default)
            where T : struct
        {
            return new FillJobArray<T>(array, value).Schedule(inputDeps);
        }

        private struct FillJobArray<T>
            : IJob
            where T : struct
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private NativeArray<T> _array;
            private readonly T _value;

            public FillJobArray(NativeArray<T> array, T value)
            {
                _array = array;
                _value = value;
            }

            public void Execute()
            {
                _array.Fill(_value);
            }
        }


        /// <summary>
        /// Schedule a job that will fill the given slice with one value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="inputDeps"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JobHandle Fill<T>(this NativeSlice<T> array, JobHandle inputDeps, T value = default)
            where T : struct
        {
            return new FillJobSlice<T>(array, value).Schedule(inputDeps);
        }

        private struct FillJobSlice<T>
            : IJob
            where T : struct
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private NativeSlice<T> _slice;
            private readonly T _value;

            public FillJobSlice(NativeSlice<T> slice, T value)
            {
                _slice = slice;
                _value = value;
            }

            public void Execute()
            {
                _slice.Fill(_value);
            }
        }
    }
}
