using System;
using Unity.Mathematics;

namespace Packages.HandyJobs.Runtime.Extensions
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

        #region sum
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
                    const int Alignment = sizeof(int) * 4;
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
        /// Sum up values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float Sum(this ReadOnlySpan<float> values)
        {
            if (values.Length == 0)
                return 0f;

            var totalSum = 0f;
            var i = 0;
            var count = values.Length;

            unsafe
            {
                fixed (float* valuesPtr = values)
                {
                    // Align to 16‑byte boundary (4 floats)
                    const int Alignment = sizeof(float) * 4; // 16
                    var offsetBytes = (int)((ulong)valuesPtr % Alignment);

                    // Number of floats to process to reach 16‑byte alignment
                    var elementsToAlign = offsetBytes == 0 ? 0 : (Alignment - offsetBytes) / sizeof(float);
                    elementsToAlign = Math.Min(elementsToAlign, count);

                    // Handle unaligned prefix
                    for (; i < elementsToAlign; i++)
                        totalSum += valuesPtr[i];

                    // SIMD loop bounds
                    var remaining = count - i;
                    var vectorCount = remaining / 4;

                    // SIMD accumulation
                    var simdSum = float4.zero;
                    var values4Ptr = (float4*)(valuesPtr + i);
                    for (var j = 0; j < vectorCount; j++)
                        simdSum += values4Ptr[j];

                    // Horizontal sum of the SIMD accumulator
                    totalSum += math.csum(simdSum);

                    // Handle remaining suffix elements
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
        public static float Sum(this Span<float> values)
        {
            return values.AsReadOnlySpan().Sum();
        }


        /// <summary>
        /// Sum up values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Sum(this ReadOnlySpan<double> values)
        {
            if (values.Length == 0)
                return 0.0;

            var totalSum = 0.0;
            var i = 0;
            var count = values.Length;

            unsafe
            {
                fixed (double* valuesPtr = values)
                {
                    // Align to 32‑byte boundary (4 doubles = 32 bytes)
                    const int Alignment = sizeof(double) * 4;
                    var offsetBytes = (int)((ulong)valuesPtr % Alignment);

                    // Number of doubles to process to reach 32‑byte alignment
                    var elementsToAlign = offsetBytes == 0 ? 0 : (Alignment - offsetBytes) / sizeof(double);
                    elementsToAlign = Math.Min(elementsToAlign, count);

                    // Handle unaligned prefix
                    for (; i < elementsToAlign; i++)
                        totalSum += valuesPtr[i];

                    // SIMD loop bounds (each vector holds 4 doubles)
                    var remaining = count - i;
                    var vectorCount = remaining / 4;

                    // SIMD accumulation
                    var simdSum = double4.zero;
                    var values4Ptr = (double4*)(valuesPtr + i);
                    for (var j = 0; j < vectorCount; j++)
                        simdSum += values4Ptr[j];

                    // Horizontal sum of the SIMD accumulator
                    totalSum += math.csum(simdSum);

                    // Handle remaining suffix elements
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
        public static double Sum(this Span<double> values)
        {
            return values.AsReadOnlySpan().Sum();
        }
        #endregion

        #region idx min/max
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
        #endregion

        /// <summary>
        /// Sum squares of values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float SumSqr(this Span<float> values)
        {
            return values.AsReadOnlySpan().SumSqr();
        }

        /// <summary>
        /// Sum squares of values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float SumSqr(this ReadOnlySpan<float> values)
        {
            if (values.Length == 0)
                return 0f;

            var count = values.Length;

            unsafe
            {
                fixed (float* valuesPtr = values)
                {
                    // Align to 16‑byte boundary (4 floats)
                    const int Alignment = sizeof(float) * 4; // 16
                    var offsetBytes = (int)((ulong)valuesPtr % Alignment);

                    // Number of floats to process to reach 16‑byte alignment
                    var elementsToAlign = offsetBytes == 0 ? 0 : (Alignment - offsetBytes) / sizeof(float);
                    elementsToAlign = Math.Min(elementsToAlign, count);

                    // Setup accumulators
                    var simdSum = float4.zero;
                    var simdComp = float4.zero;

                    // SIMD loop bounds
                    var remaining = count - elementsToAlign;
                    var vectorCount = remaining / 4;

                    // SIMD accumulation
                    var values4Ptr = (float4*)(valuesPtr + elementsToAlign);
                    for (var j = 0; j < vectorCount; j++)
                    {
                        var value4 = values4Ptr[j];
                        NeumaierSumStep(value4 * value4, ref simdSum, ref simdComp);
                    }

                    var finalSum = 0f;
                    var finalComp = 0f;

                    // Horizontal reduction of SIMD lanes
                    for (var lane = 0; lane < 4; lane++)
                        NeumaierSumStep(simdSum[lane] + simdComp[lane], ref finalSum, ref finalComp);

                    // Add on unaligned prefix
                    for (var i = 0; i < elementsToAlign; i++)
                    {
                        var value = valuesPtr[i];
                        NeumaierSumStep(value * value, ref finalSum, ref finalComp);
                    }

                    // Add on suffix
                    for (var i = elementsToAlign + vectorCount * 4; i < count; i++)
                    {
                        var value = valuesPtr[i];
                        NeumaierSumStep(value * value, ref finalSum, ref finalComp);
                    }

                    return finalSum + finalComp;
                }
            }
        }

        /// <summary>
        /// Calculate Variance over a set of values with probabilities
        /// </summary>
        /// <param name="values"></param>
        /// <param name="probabilities"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float Variance(this ReadOnlySpan<float> values, ReadOnlySpan<float> probabilities, float mean)
        {
            if (values.Length != probabilities.Length)
                throw new ArgumentException("Values.Length != probabilities.Length", nameof(probabilities));

            // Use neumaier summation, in case there is a mix of very high and very low probability events
            var sum = 0f;
            var compensator = 0f;
            for (var i = 0; i < values.Length; i++)
            {
                var delta = values[i] - mean;
                var v = probabilities[i] * delta * delta;
                NeumaierSumStep(v, ref sum, ref compensator);
            }

            return sum + compensator;
        }

        /// <summary>
        /// Calculate StdDev over a set of values with probabilities
        /// </summary>
        /// <param name="values"></param>
        /// <param name="probabilities"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float StandardDeviation(this ReadOnlySpan<float> values, Span<float> probabilities, float mean)
        {
            return math.sqrt(values.Variance(probabilities, mean));
        }

        #region Neumaier Sum
        /// <summary>
        /// Sum up input, using Neumaier summation to compensate for a mix of very large and very small values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float NeumaierSum(this Span<float> values)
        {
            return values.AsReadOnlySpan().NeumaierSum();
        }

        /// <summary>
        /// Sum up input, using Neumaier summation to compensate for a mix of very large and very small values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float NeumaierSum(this ReadOnlySpan<float> values)
        {
            if (values.Length == 0)
                return 0f;

            var count = values.Length;

            unsafe
            {
                fixed (float* valuesPtr = values)
                {
                    // Align to 16‑byte boundary (4 floats)
                    const int Alignment = sizeof(float) * 4; // 16
                    var offsetBytes = (int)((ulong)valuesPtr % Alignment);

                    // Number of floats to process to reach 16‑byte alignment
                    var elementsToAlign = offsetBytes == 0 ? 0 : (Alignment - offsetBytes) / sizeof(float);
                    elementsToAlign = Math.Min(elementsToAlign, count);

                    // Setup accumulators
                    var simdSum = float4.zero;
                    var simdComp = float4.zero;

                    // SIMD loop bounds
                    var remaining = count - elementsToAlign;
                    var vectorCount = remaining / 4;

                    // SIMD accumulation
                    var values4Ptr = (float4*)(valuesPtr + elementsToAlign);
                    for (var j = 0; j < vectorCount; j++)
                        NeumaierSumStep(values4Ptr[j], ref simdSum, ref simdComp);

                    var finalSum = 0f;
                    var finalComp = 0f;

                    // Horizontal reduction of SIMD lanes
                    for (var lane = 0; lane < 4; lane++)
                        NeumaierSumStep(simdSum[lane] + simdComp[lane], ref finalSum, ref finalComp);

                    // Add on unaligned prefix
                    for (var i = 0; i < elementsToAlign; i++)
                        NeumaierSumStep(valuesPtr[i], ref finalSum, ref finalComp);

                    // Add on suffix
                    for (var i = elementsToAlign + vectorCount * 4; i < count; i++)
                        NeumaierSumStep(valuesPtr[i], ref finalSum, ref finalComp);

                    return finalSum + finalComp;
                }
            }
        }

        public static void NeumaierSumStep(float4 value, ref float4 sum, ref float4 compensation)
        {
            var t = sum + value;

            compensation += math.select(
                (value - t) + sum,
                (sum - t) + value,
                math.abs(sum) >= math.abs(value)
            );

            sum = t;
        }

        public static void NeumaierSumStep(float value, ref float sum, ref float compensation)
        {
            var t = sum + value;

            if (Math.Abs(sum) >= Math.Abs(value))
                compensation += (sum - t) + value;
            else
                compensation += (value - t) + sum;

            sum = t;
        }
        #endregion
    }
}
