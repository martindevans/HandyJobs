using NUnit.Framework;
using System;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Tests.Collections
{
    public class JobReadIndicesExtensionsTests
    {
        #region Helper Methods
        private static int[] ToArray(NativeList<int> list)
        {
            var result = new int[list.Length];
            for (var i = 0; i < list.Length; i++)
                result[i] = list[i];
            return result;
        }

        private static (int, float)[] ToArray(NativeList<(int, float)> list)
        {
            var result = new (int, float)[list.Length];
            for (var i = 0; i < list.Length; i++)
                result[i] = list[i];
            return result;
        }

        private struct WriteSourceAndIndicesJob : IJob
        {
            private NativeArray<int> _source;
            private NativeArray<int> _indices;

            public WriteSourceAndIndicesJob(NativeArray<int> source, NativeArray<int> indices)
            {
                _source = source;
                _indices = indices;
            }

            public void Execute()
            {
                for (var i = 0; i < _source.Length; i++)
                    _source[i] = i + 1;
                _indices[0] = 3;
                _indices[1] = 1;
            }
        }

        private struct WriteTupleSourcesJob : IJob
        {
            private NativeArray<int> _source0;
            private NativeArray<float> _source1;

            public WriteTupleSourcesJob(NativeArray<int> source0, NativeArray<float> source1)
            {
                _source0 = source0;
                _source1 = source1;
            }

            public void Execute()
            {
                for (var i = 0; i < _source0.Length; i++)
                    _source0[i] = i + 1;
                for (var i = 0; i < _source1.Length; i++)
                    _source1[i] = (i + 1) * 10f;
            }
        }
        #endregion

        #region Single Source

        [Test]
        public void ReadsItemsInIndexOrder()
        {
            var source = new NativeArray<int>(new[] { 10, 20, 30, 40, 50 }, Allocator.Persistent);
            var indices = new NativeArray<int>(new[] { 3, 0, 4 }, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);

            indices.ReadIndices(source, output).Complete();

            CollectionAssert.AreEqual(new[] { 40, 10, 50 }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source.Dispose();
        }

        [Test]
        public void DuplicateIndices_ReadMultipleTimes()
        {
            var source = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var indices = new NativeArray<int>(new[] { 1, 1, 2 }, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);

            indices.ReadIndices(source, output).Complete();

            CollectionAssert.AreEqual(new[] { 20, 20, 30 }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source.Dispose();
        }

        [Test]
        public void SingleIndex()
        {
            var source = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var indices = new NativeArray<int>(new[] { 2 }, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);

            indices.ReadIndices(source, output).Complete();

            CollectionAssert.AreEqual(new[] { 30 }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source.Dispose();
        }

        [Test]
        public void AppendsToExistingOutput()
        {
            var source = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var indices = new NativeArray<int>(new[] { 0, 2 }, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(99);

            indices.ReadIndices(source, output).Complete();

            Assert.AreEqual(3, output.Length);
            Assert.AreEqual(99, output[0]);

            CollectionAssert.AreEqual(new[] { 10, 30 }, new[] { output[1], output[2] });

            output.Dispose();
            indices.Dispose();
            source.Dispose();
        }

        [Test]
        public void EmptyIndices_LeavesOutputUnchanged()
        {
            var source = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var indices = new NativeArray<int>(0, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(5);

            indices.ReadIndices(source, output).Complete();

            CollectionAssert.AreEqual(new[] { 5 }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source.Dispose();
        }

        [Test]
        public void LargeSource_ScatteredIndices()
        {
            var count = 10000;
            var indexCount = 1000;
            var source = new NativeArray<int>(count, Allocator.Persistent);
            for (var i = 0; i < count; i++)
                source[i] = i * 3 + 1;

            var indices = new NativeArray<int>(indexCount, Allocator.Persistent);
            var expected = new int[indexCount];
            for (var i = 0; i < indexCount; i++)
            {
                var idx = (i * 997) % count;
                indices[i] = idx;
                expected[i] = source[idx];
            }

            var output = new NativeList<int>(Allocator.Persistent);

            indices.ReadIndices(source, output).Complete();

            CollectionAssert.AreEqual(expected, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source.Dispose();
        }

        [Test]
        public void WaitsForDependentJob()
        {
            var source = new NativeArray<int>(5, Allocator.Persistent);
            var indices = new NativeArray<int>(2, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);

            var writeJob = new WriteSourceAndIndicesJob(source, indices).Schedule();
            var readJob = indices.ReadIndices(source, output, writeJob);

            readJob.Complete();

            CollectionAssert.AreEqual(new[] { 4, 2 }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source.Dispose();
        }

        #endregion

        #region Two Sources

        [Test]
        public void TwoSources_ReadsPairsInIndexOrder()
        {
            var source0 = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var source1 = new NativeArray<float>(new[] { 100f, 200f, 300f }, Allocator.Persistent);
            var indices = new NativeArray<(int, int)>(new[] { (2, 0), (0, 1) }, Allocator.Persistent);
            var output = new NativeList<(int, float)>(Allocator.Persistent);

            indices.ReadIndices(source0, source1, output).Complete();

            CollectionAssert.AreEqual(new (int, float)[] { (30, 100f), (10, 200f) }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source1.Dispose();
            source0.Dispose();
        }

        [Test]
        public void TwoSources_DuplicateIndices_ReadMultipleTimes()
        {
            var source0 = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var source1 = new NativeArray<float>(new[] { 1f, 2f, 3f }, Allocator.Persistent);
            var indices = new NativeArray<(int, int)>(new[] { (1, 1), (1, 1) }, Allocator.Persistent);
            var output = new NativeList<(int, float)>(Allocator.Persistent);

            indices.ReadIndices(source0, source1, output).Complete();

            CollectionAssert.AreEqual(new (int, float)[] { (20, 2f), (20, 2f) }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source1.Dispose();
            source0.Dispose();
        }

        [Test]
        public void TwoSources_AppendsToExistingOutput()
        {
            var source0 = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var source1 = new NativeArray<float>(new[] { 1f, 2f, 3f }, Allocator.Persistent);
            var indices = new NativeArray<(int, int)>(new[] { (0, 0) }, Allocator.Persistent);
            var output = new NativeList<(int, float)>(Allocator.Persistent);
            output.Add((99, 99f));

            indices.ReadIndices(source0, source1, output).Complete();

            Assert.AreEqual(2, output.Length);
            Assert.AreEqual((99, 99f), output[0]);
            Assert.AreEqual((10, 1f), output[1]);

            output.Dispose();
            indices.Dispose();
            source1.Dispose();
            source0.Dispose();
        }

        [Test]
        public void TwoSources_EmptyIndices_LeavesOutputUnchanged()
        {
            var source0 = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var source1 = new NativeArray<float>(new[] { 1f, 2f, 3f }, Allocator.Persistent);
            var indices = new NativeArray<(int, int)>(0, Allocator.Persistent);
            var output = new NativeList<(int, float)>(Allocator.Persistent);
            output.Add((5, 5f));

            indices.ReadIndices(source0, source1, output).Complete();

            CollectionAssert.AreEqual(new (int, float)[] { (5, 5f) }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source1.Dispose();
            source0.Dispose();
        }

        [Test]
        public void TwoSources_WaitsForDependentJob()
        {
            var source0 = new NativeArray<int>(3, Allocator.Persistent);
            var source1 = new NativeArray<float>(3, Allocator.Persistent);
            var indices = new NativeArray<(int, int)>(new[] { (1, 0), (0, 2) }, Allocator.Persistent);
            var output = new NativeList<(int, float)>(Allocator.Persistent);

            var writeJob = new WriteTupleSourcesJob(source0, source1).Schedule();
            var readJob = indices.ReadIndices(source0, source1, output, writeJob);

            readJob.Complete();

            CollectionAssert.AreEqual(new (int, float)[] { (2, 10f), (1, 30f) }, ToArray(output));

            output.Dispose();
            indices.Dispose();
            source1.Dispose();
            source0.Dispose();
        }

        #endregion
    }
}
