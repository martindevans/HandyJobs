using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Extensions
{
    public static class NativeArrayExtensions
    {
        /// <summary>
        /// Returns a new array of the given size with the contents of this
        /// array copied into it. The original array is disposed.
        /// </summary>
        public static NativeArray<T> Resize<T>(this NativeArray<T> array, int size, T fill, Allocator allocator)
            where T : struct
        {
            if (size < 0)
                throw new ArgumentException("Size must be non-negative", nameof(size));

            // Nothing to do
            if (size == array.Length)
                return array;

            // Create array filled with default value
            var result = new NativeArray<T>(size, allocator, NativeArrayOptions.UninitializedMemory);
            result.AsSpan().Fill(fill);

            // Copy data
            var count = math.min(size, array.Length);
            if (count > 0)
                array.AsSpan()[..count].CopyTo(result.AsSpan()[..count]);

            // Dispose old array
            array.Dispose();

            return result;
        }
    }
}
