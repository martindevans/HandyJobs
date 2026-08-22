using Unity.Collections;

namespace Packages.HandyJobs.Runtime.Extensions
{
    public static class NativeArrayExtensions
    {
        /// <summary>
        /// Fill the array entirely with one value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="value"></param>
        public static void Fill<T>(this NativeArray<T> array, T value)
            where T : struct
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = value;
        }
    }
}
