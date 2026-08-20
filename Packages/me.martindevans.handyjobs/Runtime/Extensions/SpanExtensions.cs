using System;
using Unity.Mathematics;

namespace me.martindevans.handyjobs.Extensions
{
    public static class SpanExtensions
    {
        #region AsReadOnlySpan
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this Span<T> span)
        {
            return span;
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] arr)
        {
            return arr;
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] arr, int start)
        {
            return arr.AsSpan(start);
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] arr, int start, int count)
        {
            return arr.AsSpan(start, count);
        }
        #endregion

        /// <summary>
        /// Sum up values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int Sum(this ReadOnlySpan<int> values)
        {
            if (values.Length == 0)
                return 0;

            var totalSum = 0;
            var i = 0;
            var count = values.Length;

            unsafe
            {
                fixed (int* valuesPtr = values)
                {
                    // Align to 16-byte boundary (4xInteger)
                    const int Alignment = 16;
                    var offsetBytes = (int)((ulong)valuesPtr % Alignment);

                    // Calculate how many ints to process to reach alignment
                    // If offset is 4 bytes, we need 3 more ints (12 bytes) to hit 16
                    var elementsToAlign = offsetBytes == 0 ? 0 : (Alignment - offsetBytes) / sizeof(int);
                    elementsToAlign = Math.Min(elementsToAlign, count);

                    // Handle the prefix
                    for (; i < elementsToAlign; i++)
                        totalSum += valuesPtr[i];

                    // Calculate SIMD loop bounds
                    var remaining = count - i;
                    var vectorCount = remaining / 4;

                    // Actual SIMD loop
                    var simdSum = int4.zero;
                    var values4Ptr = (int4*)(valuesPtr + i);
                    for (var j = 0; j < vectorCount; j++)
                        simdSum += values4Ptr[j];

                    // Add the SIMD elements to the total
                    totalSum += math.csum(simdSum);

                    // Handle the suffix elements
                    i += vectorCount * 4;
                    for (; i < count; i++)
                        totalSum += valuesPtr[i];
                }
            }

            return totalSum;
        }

        /// <summary>
        /// Sum up values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int Sum(this Span<int> values)
        {
            return values.AsReadOnlySpan().Sum();
        }

        /// <summary>
        /// Get index of max value
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int IdxMax<T>(this ReadOnlySpan<T> values)
            where T : IComparable<T>
        {
            var idx = 0;
            var max = values[0];
            for (var i = 1; i < values.Length; i++)
            {
                if (values[i].IsGreaterThan(max))
                {
                    max = values[i];
                    idx = i;
                }
            }

            return idx;
        }

        /// <summary>
        /// Get index of min value
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int IdxMin<T>(this ReadOnlySpan<T> values)
            where T : IComparable<T>
        {
            var idx = 0;
            var min = values[0];
            for (var i = 1; i < values.Length; i++)
            {
                if (values[i].IsLessThan(min))
                {
                    min = values[i];
                    idx = i;
                }
            }

            return idx;
        }
    }
}
