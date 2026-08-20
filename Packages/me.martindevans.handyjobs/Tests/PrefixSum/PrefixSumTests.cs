using NUnit.Framework;
using System;
using me.martindevans.handyjobs.Primitives.Mathematics;
using Unity.Collections;

namespace Tests.PrefixSum
{
    public class PrefixSumTests
    {
        #region Helper Methods
        private static int[] RunPrefixSum(int[] rawValues)
        {
            var values = new NativeArray<int>(rawValues, Allocator.Persistent);
            var output = new NativeArray<int>(rawValues.Length, Allocator.Persistent);

            values.PrefixSum(output, default).Complete();

            var result = new int[rawValues.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = output[i];

            values.Dispose();
            output.Dispose();
            return result;
        }

        private static void AssertMatchesReference(int[] values, int[] result)
        {
            var sum = 0;
            for (var i = 0; i < values.Length; i++)
            {
                sum += values[i];
                Assert.AreEqual(sum, result[i], $"Mismatch at index {i}");
            }
        }
        #endregion

        [Test]
        public void BasicPrefixSum()
        {
            var values = new NativeArray<int>(new[]
            {
                1, 2, 3,
                2, 3, 1,
                3, 1, 2,
            }, Allocator.Persistent);

            var expected = new[]
            {
                1, 3, 6, 8, 11, 12, 15, 16, 18
            };

            var output = new NativeArray<int>(9, Allocator.Persistent);

            values.PrefixSum(output, default).Complete();

            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], output[i]);

            values.Dispose();
            output.Dispose();
        }

        #region Input Validation

        [Test]
        public void Throws_MismatchedLengths()
        {
            var values = new NativeArray<int>(3, Allocator.Persistent);
            var output = new NativeArray<int>(4, Allocator.Persistent);

            Assert.That(() => values.PrefixSum(output, default).Complete(),
                Throws.InstanceOf<ArgumentException>());

            values.Dispose();
            output.Dispose();
        }

        #endregion

        #region Edge Cases

        [Test]
        public void EmptyArrays()
        {
            var values = new NativeArray<int>(0, Allocator.Persistent);
            var output = new NativeArray<int>(0, Allocator.Persistent);

            values.PrefixSum(output, default).Complete();

            Assert.IsEmpty(output);

            values.Dispose();
            output.Dispose();
        }

        [Test]
        public void SingleElement()
        {
            var result = RunPrefixSum(new[] { 42 });

            Assert.AreEqual(42, result[0]);
        }

        [Test]
        public void AllZeros()
        {
            var values = new int[100];
            var result = RunPrefixSum(values);

            for (var i = 0; i < result.Length; i++)
                Assert.AreEqual(0, result[i], $"Expected 0 at index {i}");
        }

        [Test]
        public void NegativeValues()
        {
            var rawValues = new[] { -1, -5, 10, -3, 2 };
            var result = RunPrefixSum(rawValues);

            AssertMatchesReference(rawValues, result);
        }

        #endregion

        #region Block Boundaries

        [Test]
        public void ExactBlockSize()
        {
            var rawValues = new int[128];
            for (var i = 0; i < rawValues.Length; i++)
                rawValues[i] = 1;

            var result = RunPrefixSum(rawValues);

            AssertMatchesReference(rawValues, result);
        }

        [Test]
        public void JustOverBlockSize()
        {
            var rawValues = new int[129];
            for (var i = 0; i < rawValues.Length; i++)
                rawValues[i] = 1;

            var result = RunPrefixSum(rawValues);

            AssertMatchesReference(rawValues, result);
        }

        [Test]
        public void MultipleBlocks()
        {
            var rawValues = new int[300];
            for (var i = 0; i < rawValues.Length; i++)
                rawValues[i] = i % 7 - 3;

            var result = RunPrefixSum(rawValues);

            AssertMatchesReference(rawValues, result);
        }

        #endregion
    }
}
