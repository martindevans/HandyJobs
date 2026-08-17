using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Primitives
{
    /// <summary>
    /// Runs a scan over an array, calculating a prefix sum. I.e. the sum of everything in the array up to that point.
    /// </summary>
    public static class PrefixSumExtensions
    {
        private const int BlockSize = 128;

        /// <summary>
        /// Parallel calculation of prefix sum. For each element, stores the sum of all previous elements and itself.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static JobHandle PrefixSum(
            this NativeArray<int> input,
            NativeArray<int> output,
            JobHandle dependency = default)
        {
            // Sanity check inputs
            if (input.Length != output.Length)
                throw new System.ArgumentException("Input and output lengths must match.");

            // Early exit
            if (input.Length == 0)
            {
                output.AsSpan().Fill(0);
                return dependency;
            }

            // Calculate total bloc count
            var blockCount = (input.Length + BlockSize - 1) / BlockSize;

            // Allocate temporary space
            var blockSums = new NativeArray<int>(blockCount, Allocator.TempJob);
            var blockOffsets = new NativeArray<int>(blockCount, Allocator.TempJob);

            // Compute the prefix sum within each block, writing the partial results to Output.
            // Also record each blocks total sum in BlockSums.
            var handle = new ScanBlocksJob(input, output, blockSums).Schedule(blockCount, 1, dependency);

            // Computes the prefix sum of the block totals, producing the starting offset for each block.
            handle = new ScanBlockSumsJob(blockSums, blockOffsets).Schedule(handle);

            // Adds each block’s accumulated offset to its locally scanned values.
            // This converts the per-block prefix sums into the final global prefix sum.
            handle = new AddBlockOffsetsJob(output, blockOffsets).Schedule(input.Length, 64, handle);

            // Clean up
            handle = blockSums.Dispose(handle);
            handle = blockOffsets.Dispose(handle);

            return handle;
        }

        [BurstCompile]
        private struct ScanBlocksJob
            : IJobParallelFor
        {
            [ReadOnly] private readonly NativeArray<int> Input;

            [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<int> Output;
            [WriteOnly] private NativeArray<int> BlockSums;

            public ScanBlocksJob(NativeArray<int> input, NativeArray<int> output, NativeArray<int> blockSums)
            {
                Input = input;
                Output = output;
                BlockSums = blockSums;
            }

            public void Execute(int blockIndex)
            {
                var start = blockIndex * BlockSize;
                var end = math.min(start + BlockSize, Input.Length);

                // Calculate prefix sum for this block
                var sum = 0;
                for (var i = start; i < end; i++)
                {
                    sum += Input[i];
                    Output[i] = sum;
                }

                // Store final total for this block
                BlockSums[blockIndex] = sum;
            }
        }

        [BurstCompile]
        private struct ScanBlockSumsJob
            : IJob
        {
            [ReadOnly] private readonly NativeArray<int> BlockSums;
            [WriteOnly] private NativeArray<int> BlockOffsets;

            public ScanBlockSumsJob(NativeArray<int> blockSums, NativeArray<int> blockOffsets)
            {
                BlockSums = blockSums;
                BlockOffsets = blockOffsets;
            }

            public void Execute()
            {
                var sum = 0;

                for (var i = 0; i < BlockSums.Length; i++)
                {
                    BlockOffsets[i] = sum;
                    sum += BlockSums[i];
                }
            }
        }

        [BurstCompile]
        private struct AddBlockOffsetsJob
            : IJobParallelFor
        {
            private NativeArray<int> Output;
            [ReadOnly] private readonly NativeArray<int> BlockOffsets;

            public AddBlockOffsetsJob(NativeArray<int> output, NativeArray<int> blockOffsets)
            {
                Output = output;
                BlockOffsets = blockOffsets;
            }

            public void Execute(int index)
            {
                Output[index] += BlockOffsets[index / BlockSize];
            }
        }
    }
}