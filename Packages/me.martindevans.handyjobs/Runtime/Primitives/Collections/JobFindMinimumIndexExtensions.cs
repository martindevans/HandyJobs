using System;
using me.martindevans.handyjobs.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace me.martindevans.handyjobs.Primitives.Collections
{
    [BurstCompile]
    public static class JobFindMinimumIndexExtensions
    {
        /// <summary>
        /// Find the N smallest items from the input. Writing to the output array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle FindMinIndices<T>(this NativeArray<T> input, NativeArray<int> output, JobHandle dependsOn = default)
            where T : unmanaged, IComparable<T>
        {
            // Allocate space for each thread to independently find minimums
            var threadMinimums = new NativeArray<int>(JobsUtility.MaxJobThreadCount * output.Length, Allocator.Persistent);
            dependsOn = threadMinimums.Fill(dependsOn, -1);

            // Schedule job to find minimums
            dependsOn = new FindMinimaJob<T>(input, threadMinimums, output.Length).Schedule(input.Length, 32, dependsOn);

            // Merge per-thread minimums
            dependsOn = new MergeMinimaJob<T>(input, threadMinimums, output).Schedule(dependsOn);
            dependsOn = threadMinimums.Dispose(dependsOn);

            return dependsOn;
        }

        private static void Insert<T>(Span<int> outputIndices, NativeArray<T> items, int index)
            where T : struct, IComparable<T>
        {
            if (index < 0 || index >= items.Length)
                return;

            // Get the item we might be inserting
            var item = items[index];

            for (var i = 0; i < outputIndices.Length; i++)
            {
                var otherIndex = outputIndices[i];
                if (otherIndex == -1 || item.IsLessThan(items[otherIndex]))
                {
                    // Shift elements to the right to make room
                    for (var j = outputIndices.Length - 1; j > i; j--)
                        outputIndices[j] = outputIndices[j - 1];

                    // Insert the new minimum and stop
                    outputIndices[i] = index;
                    break;
                }
            }
        }

        [BurstCompile]
        private struct FindMinimaJob<T>
            : IJobParallelFor
            where T : struct, IComparable<T>
        {
            [ReadOnly] private readonly NativeArray<T> _input;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            [NativeDisableParallelForRestriction] private NativeArray<int> _threadMinimums;
            private readonly int _minimaCount;

            public FindMinimaJob(NativeArray<T> input, NativeArray<int> threadMinimums, int minimaCount)
            {
                _input = input;
                _threadMinimums = threadMinimums;
                _minimaCount = minimaCount;
            }

            public void Execute(int index)
            {
                var slice = _threadMinimums.AsSpan().Slice(JobsUtility.ThreadIndex * _minimaCount, _minimaCount);
                Insert(slice, _input, index);
            }
        }

        [BurstCompile]
        private struct MergeMinimaJob<T>
            : IJob
            where T : struct, IComparable<T>
        {
            [ReadOnly] private readonly NativeArray<T> _values;
            private readonly NativeArray<int> _perThreadMinima;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private NativeArray<int> _output;

            public MergeMinimaJob(NativeArray<T> values, NativeArray<int> perThreadMinima, NativeArray<int> output)
            {
                _values = values;
                _perThreadMinima = perThreadMinima;
                _output = output;
            }

            public void Execute()
            {
                // Fill output buffer with -1
                _output.Fill(-1);

                // Select the most minimal minima over the thread buffer
                for (var i = 0; i < _perThreadMinima.Length; i++)
                    Insert(_output.AsSpan(), _values, _perThreadMinima[i]);
            }
        }

        /// <summary>
        /// Find the index of the smallest item in the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="dependsOn"></param>
        /// <returns></returns>
        public static JobHandle FindMinIndex<T>(this NativeArray<T> input, NativeReference<int> output, JobHandle dependsOn = default)
            where T : unmanaged, IComparable<T>
        {
            // Allocate space for each thread to independently find minimums
            var threadMinimums = new NativeArray<int>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
            dependsOn = threadMinimums.Fill(dependsOn, -1);

            // Schedule job to find minimums
            dependsOn = new FindSingleMinimaJob<T>(input, threadMinimums).Schedule(input.Length, 32, dependsOn);

            // Merge per-thread minimums
            dependsOn = new MergeSingleMinimaJob<T>(input, threadMinimums, output).Schedule(dependsOn);
            dependsOn = threadMinimums.Dispose(dependsOn);

            return dependsOn;
        }

        [BurstCompile]
        private struct FindSingleMinimaJob<T>
            : IJobParallelForBatch
            where T : struct, IComparable<T>
        {
            [ReadOnly] private readonly NativeArray<T> _input;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            [NativeDisableParallelForRestriction] private NativeArray<int> _threadMinimums;

            public FindSingleMinimaJob(NativeArray<T> input, NativeArray<int> threadMinimums)
            {
                _input = input;
                _threadMinimums = threadMinimums;
            }

            public void Execute(int startIndex, int count)
            {
                // Get the minimum index in this slice
                var minIdx = startIndex + _input.AsReadOnlySpan().Slice(startIndex, count).IdxMin();
                var item = _input[minIdx];

                // Current best index found by this thread so far
                ref var currentBestIdx = ref _threadMinimums.AsSpan()[JobsUtility.ThreadIndex];

                // Check if the current best is better, if so early exit
                if (currentBestIdx != -1)
                {
                    var currentBestItem = _input[currentBestIdx];
                    var cmp = currentBestItem.CompareTo(item);

                    // If current best is smaller early exit
                    if (cmp < 0)
                        return;

                    // if current best is equal, keep it if it's a smaller index
                    if (cmp == 0)
                        if (currentBestIdx < minIdx)
                            return;
                }

                // Update the current best
                currentBestIdx = minIdx;
            }
        }

        [BurstCompile]
        private struct MergeSingleMinimaJob<T>
            : IJob
            where T : unmanaged, IComparable<T>
        {
            [ReadOnly] private readonly NativeArray<T> _items;
            private readonly NativeArray<int> _threadMinimums;

            private NativeReference<int> _output;

            public MergeSingleMinimaJob(NativeArray<T> items, NativeArray<int> threadMinimums, NativeReference<int> output)
            {
                _items = items;
                _threadMinimums = threadMinimums;
                _output = output;
            }

            public void Execute()
            {
                // Early exit for empty array
                if (_items.Length == 0)
                {
                    _output.Value = -1;
                    return;
                }

                unsafe
                {
                    var itemsPtr = (T*)_items.GetUnsafeReadOnlyPtr();

                    // Assume index 0 is the smallest item
                    ref var bestItem = ref itemsPtr[0];
                    var bestIdx = 0;

                    // Loop over the best item found by each thread
                    for (var i = 0; i < _threadMinimums.Length; i++)
                    {
                        var idx = _threadMinimums[i];
                        if (idx < 0)
                            continue;
                        if (idx >= _items.Length)
                            continue;

                        ref var item = ref itemsPtr[idx];

                        // Compare items
                        var cmp = bestItem.CompareTo(item);

                        if (cmp > 0)
                        {
                            // New item is smaller
                            bestItem = item;
                            bestIdx = idx;
                        }
                        else if (cmp == 0)
                        {
                            // new item is equal, keep smallest index
                            if (idx < bestIdx)
                            {
                                bestItem = item;
                                bestIdx = idx;
                            }
                        }
                    }

                    _output.Value = bestIdx;
                }
            }
        }
    }
}
