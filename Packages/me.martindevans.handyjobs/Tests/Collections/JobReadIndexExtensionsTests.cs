using NUnit.Framework;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Tests.Collections
{
    public class JobReadIndexExtensionsTests
    {
        private const int Sentinel = 999;

        #region Helper Methods
        private struct WriteArrayJob : IJob
        {
            private NativeArray<int> _array;

            public WriteArrayJob(NativeArray<int> array)
            {
                _array = array;
            }

            public void Execute()
            {
                for (var i = 0; i < _array.Length; i++)
                    _array[i] = i * 10;
            }
        }

        private struct WriteListJob : IJob
        {
            private NativeList<int> _list;
            private readonly int _count;

            public WriteListJob(NativeList<int> list, int count)
            {
                _list = list;
                _count = count;
            }

            public void Execute()
            {
                for (var i = 0; i < _count; i++)
                    _list.Add(i * 10);
            }
        }
        #endregion

        #region NativeArray

        [Test]
        public void Array_ReadsElementAtPositiveIndex()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30, 40 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 2).Complete();

            Assert.AreEqual(30, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_ReadsFirstElement()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 0).Complete();

            Assert.AreEqual(10, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_ReadsLastElement()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 2).Complete();

            Assert.AreEqual(30, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_ReadsElementWithNegativeIndex()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30, 40 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, ^1).Complete();

            Assert.AreEqual(40, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_ReadsElementWithNegativeIndex_SecondFromEnd()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30, 40 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, ^2).Complete();

            Assert.AreEqual(30, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_IndexEqualToLength_DoesNotWrite()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 3).Complete();

            Assert.AreEqual(Sentinel, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_IndexBeyondLength_DoesNotWrite()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 10).Complete();

            Assert.AreEqual(Sentinel, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_IndexLessThanNegativeLength_DoesNotWrite()
        {
            var input = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, ^4).Complete();

            Assert.AreEqual(Sentinel, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_EmptyInput_DoesNotWrite()
        {
            var input = new NativeArray<int>(0, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 0).Complete();

            Assert.AreEqual(Sentinel, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void Array_WaitsForDependentJob()
        {
            var input = new NativeArray<int>(5, Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            var writeJob = new WriteArrayJob(input).Schedule();
            var readJob = input.ReadIndex(output, 3, writeJob);

            readJob.Complete();

            Assert.AreEqual(30, output.Value);

            output.Dispose();
            input.Dispose();
        }

        #endregion

        #region NativeList

        [Test]
        public void List_ReadsElementAtPositiveIndex()
        {
            var input = new NativeList<int>(Allocator.Persistent);
            input.Add(10);
            input.Add(20);
            input.Add(30);

            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 1).Complete();

            Assert.AreEqual(20, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void List_ReadsLastElement()
        {
            var input = new NativeList<int>(Allocator.Persistent);
            input.Add(10);
            input.Add(20);

            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 1).Complete();

            Assert.AreEqual(20, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void List_ReadsElementWithNegativeIndex()
        {
            var input = new NativeList<int>(Allocator.Persistent);
            input.Add(10);
            input.Add(20);
            input.Add(30);

            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, ^1).Complete();

            Assert.AreEqual(30, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void List_IndexEqualToLength_DoesNotWrite()
        {
            var input = new NativeList<int>(Allocator.Persistent);
            input.Add(10);
            input.Add(20);

            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 2).Complete();

            Assert.AreEqual(Sentinel, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void List_IndexLessThanNegativeLength_DoesNotWrite()
        {
            var input = new NativeList<int>(Allocator.Persistent);
            input.Add(10);
            input.Add(20);

            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, ^3).Complete();

            Assert.AreEqual(Sentinel, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void List_EmptyInput_DoesNotWrite()
        {
            var input = new NativeList<int>(Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            input.ReadIndex(output, 0).Complete();

            Assert.AreEqual(Sentinel, output.Value);

            output.Dispose();
            input.Dispose();
        }

        [Test]
        public void List_WaitsForDependentJob()
        {
            var input = new NativeList<int>(Allocator.Persistent);
            var output = new NativeReference<int>(Sentinel, Allocator.Persistent);

            var writeJob = new WriteListJob(input, 5).Schedule();
            var readJob = input.ReadIndex(output, 2, writeJob);

            readJob.Complete();

            Assert.AreEqual(20, output.Value);

            output.Dispose();
            input.Dispose();
        }

        #endregion
    }
}
