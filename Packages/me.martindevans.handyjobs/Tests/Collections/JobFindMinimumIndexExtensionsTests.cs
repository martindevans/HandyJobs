using NUnit.Framework;
using System;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Tests.Collections
{
    public class JobFindMinimumIndexExtensionsTests
    {
        #region Helper Methods
        private static int[] ReferenceTopNIndices(int[] values, int n)
        {
            var indices = new int[values.Length];
            for (var i = 0; i < indices.Length; i++)
                indices[i] = i;

            Array.Sort(indices, (a, b) => values[a].CompareTo(values[b]));

            var count = Math.Min(n, values.Length);
            var result = new int[count];
            Array.Copy(indices, result, count);
            return result;
        }

        private static void AssertDistinctValidIndices(NativeArray<int> output, int inputLength)
        {
            var seen = new bool[inputLength];
            for (var i = 0; i < output.Length; i++)
            {
                var index = output[i];
                if (index == -1)
                    continue;

                Assert.That(index, Is.GreaterThanOrEqualTo(0).And.LessThan(inputLength),
                    $"Index {index} at output slot {i} is out of range");
                Assert.IsFalse(seen[index], $"Index {index} appears more than once in output");
                seen[index] = true;
            }
        }

        private struct WriteMinPatternJob : IJob
        {
            private NativeArray<int> _array;
            private readonly int _minIndex;

            public WriteMinPatternJob(NativeArray<int> array, int minIndex)
            {
                _array = array;
                _minIndex = minIndex;
            }

            public void Execute()
            {
                for (var i = 0; i < _array.Length; i++)
                    _array[i] = i == _minIndex ? -1000 : i;
            }
        }

        private struct WriteTopPatternJob : IJob
        {
            private NativeArray<int> _array;
            private NativeArray<int> _specialIndices;

            public WriteTopPatternJob(NativeArray<int> array, NativeArray<int> specialIndices)
            {
                _array = array;
                _specialIndices = specialIndices;
            }

            public void Execute()
            {
                for (var i = 0; i < _array.Length; i++)
                    _array[i] = 10000 + i;
                for (var i = 0; i < _specialIndices.Length; i++)
                    _array[_specialIndices[i]] = i;
            }
        }

        private static void FindMinIndex_FuzzIteration(int seed)
        {
            var random = new Random(unchecked(seed * 34958720));
            var length = random.Next(0, 1024);
            var valueRange = random.Next(1, 100);

            var raw = new int[length];
            for (var i = 0; i < length; i++)
                raw[i] = random.Next(-valueRange, valueRange);

            var expected = -1;
            for (var i = 0; i < raw.Length; i++)
            {
                if (expected == -1 || raw[i] < raw[expected])
                    expected = i;
            }

            var input = new NativeArray<int>(raw, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(expected, output.Value,
                $"seed {seed}: length {length}, valueRange {valueRange}");

            output.Dispose();
            input.Dispose();
        }
        #endregion

        #region FindMinIndex

        [Test]
        public void FindMinIndex_SingleElement()
        {
            var input = new NativeArray<int>(new[] { 42 }, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(0, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_MinimumAtStart()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(0, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_MinimumAtEnd()
        {
            var input = new NativeArray<int>(new[] { 4, 3, 2, 1 }, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(3, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_MinimumInMiddle()
        {
            var input = new NativeArray<int>(new[] { 4, 3, 1, 5, 2 }, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(2, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_AllEqualValues_ReturnsFirstIndex()
        {
            var input = new NativeArray<int>(new[] { 7, 7, 7, 7 }, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(0, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_DuplicateMinima_ReturnsFirstIndex()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2, 1, 0, 0, 4 }, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(4, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_NegativeValues()
        {
            var input = new NativeArray<int>(new[] { -5, -10, -3, -10, -1 }, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(1, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_EmptyArray_ReturnsNegativeOne()
        {
            var input = new NativeArray<int>(0, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(-1, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_LargeArray()
        {
            var raw = new int[100000];
            for (var i = 0; i < raw.Length; i++)
                raw[i] = (i * 7 + 3) % 97;

            var expected = 0;
            for (var i = 1; i < raw.Length; i++)
                if (raw[i] < raw[expected])
                    expected = i;

            var input = new NativeArray<int>(raw, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            input.FindMinIndex(output).Complete();

            Assert.AreEqual(expected, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_WaitsForDependentJob()
        {
            var input = new NativeArray<int>(1000, Allocator.Persistent);
            var output = new NativeReference<int>(Allocator.Persistent);

            var writeJob = new WriteMinPatternJob(input, 500).Schedule();
            input.FindMinIndex(output, writeJob).Complete();

            Assert.AreEqual(500, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndex_Fuzz()
        {
            for (var seed = 0; seed < 100; seed++)
                FindMinIndex_FuzzIteration(seed);
        }

        #endregion

        #region FindMinIndices

        [Test]
        public void FindMinIndices_FindsSingleMinimum()
        {
            var input = new NativeArray<int>(new[] { 4, 3, 1, 5, 2 }, Allocator.Persistent);
            var output = new NativeArray<int>(1, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            Assert.AreEqual(2, output[0]);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_ReturnsSortedIndices()
        {
            var input = new NativeArray<int>(new[] { 5, 3, 1, 4, 2 }, Allocator.Persistent);
            var output = new NativeArray<int>(3, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            Assert.AreEqual(2, output[0]);
            Assert.AreEqual(4, output[1]);
            Assert.AreEqual(1, output[2]);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_OutputLargerThanInput_PadsWithNegativeOne()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2 }, Allocator.Persistent);
            var output = new NativeArray<int>(5, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            Assert.AreEqual(1, output[0]);
            Assert.AreEqual(2, output[1]);
            Assert.AreEqual(0, output[2]);
            Assert.AreEqual(-1, output[3]);
            Assert.AreEqual(-1, output[4]);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_OutputEqualsInput_ReturnsAllSorted()
        {
            var input = new NativeArray<int>(new[] { 4, 1, 3, 2 }, Allocator.Persistent);
            var output = new NativeArray<int>(4, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            Assert.AreEqual(1, output[0]);
            Assert.AreEqual(3, output[1]);
            Assert.AreEqual(2, output[2]);
            Assert.AreEqual(0, output[3]);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_AllEqualValues_ReturnsDistinctIndices()
        {
            var input = new NativeArray<int>(new[] { 5, 5, 5, 5 }, Allocator.Persistent);
            var output = new NativeArray<int>(2, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            AssertDistinctValidIndices(output, input.Length);
            Assert.AreNotEqual(-1, output[0], "Expected a valid index in output slot 0");
            Assert.AreNotEqual(-1, output[1], "Expected a valid index in output slot 1");

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_Ties_ReturnsCorrectValues()
        {
            var input = new NativeArray<int>(new[] { 2, 1, 2, 1 }, Allocator.Persistent);
            var output = new NativeArray<int>(3, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            AssertDistinctValidIndices(output, input.Length);
            Assert.AreEqual(1, input[output[0]]);
            Assert.AreEqual(1, input[output[1]]);
            Assert.AreEqual(2, input[output[2]]);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_EmptyInput_ReturnsAllNegativeOne()
        {
            var input = new NativeArray<int>(0, Allocator.Persistent);
            var output = new NativeArray<int>(3, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            Assert.AreEqual(-1, output[0]);
            Assert.AreEqual(-1, output[1]);
            Assert.AreEqual(-1, output[2]);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_SingleInput()
        {
            var input = new NativeArray<int>(new[] { 7 }, Allocator.Persistent);
            var output = new NativeArray<int>(3, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            Assert.AreEqual(0, output[0]);
            Assert.AreEqual(-1, output[1]);
            Assert.AreEqual(-1, output[2]);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_LargeArray()
        {
            var raw = new int[100000];
            for (var i = 0; i < raw.Length; i++)
                raw[i] = (int)((ulong)i * 2654435761ul % 1000000ul);

            var expected = ReferenceTopNIndices(raw, 10);

            var input = new NativeArray<int>(raw, Allocator.Persistent);
            var output = new NativeArray<int>(10, Allocator.Persistent);

            input.FindMinIndices(output).Complete();

            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], output[i], $"Mismatch at output slot {i}");

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void FindMinIndices_WaitsForDependentJob()
        {
            var input = new NativeArray<int>(1000, Allocator.Persistent);
            var specialIndices = new NativeArray<int>(new[] { 29, 13, 7 }, Allocator.Persistent);
            var output = new NativeArray<int>(3, Allocator.Persistent);

            var writeJob = new WriteTopPatternJob(input, specialIndices).Schedule();
            input.FindMinIndices(output, writeJob).Complete();

            Assert.AreEqual(29, output[0]);
            Assert.AreEqual(13, output[1]);
            Assert.AreEqual(7, output[2]);

            output.Dispose();
            specialIndices.Dispose();
            input.Dispose();
        }

        #endregion
    }
}
