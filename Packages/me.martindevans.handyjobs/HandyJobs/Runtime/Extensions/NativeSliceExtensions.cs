using Unity.Collections;

namespace Packages.HandyJobs.Runtime.Extensions
{
    public static class NativeSliceExtensions
    {
        /// <summary>
        /// Returns a new array of the given size with the contents of this
        /// array copied into it. The original array is disposed.
        /// </summary>
        public static void Fill<T>(this NativeSlice<T> slice, T fill)
            where T : struct
        {
            for (var i = 0; i < slice.Length; i++)
                slice[i] = fill;
        }
    }
}
