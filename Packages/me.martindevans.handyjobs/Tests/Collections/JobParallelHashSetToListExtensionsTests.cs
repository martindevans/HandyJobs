using NUnit.Framework;
using System;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Tests.Collections
{
    public class JobParallelHashSetToListExtensionsTests
    {
        #region Helper Methods
        private static int[] ToArray(NativeList<int> list)
        {
            var result = new int[list.Length];
            for (var i = 0; i < list.Length; i++)
                result[i] = list[i];
            return result;
        }

        private static void AssertContainsExactly(int[] actual, int[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Length,
                $"Expected {expected.Length} elements but list has {actual.Length}");

            var sortedExpected = (int[])expected.Clone();
            Array.Sort(sortedExpected);

            var sortedActual = (int[])actual.Clone();
            Array.Sort(sortedActual);

            for (var i = 0; i < sortedActual.Length; i++)
                Assert.AreEqual(sortedExpected[i], sortedActual[i], $"Mismatch at sorted index {i}");
        }

        private struct WriteSetJob : IJob
        {
            private NativeParallelHashSet<int> _set;
            private NativeArray<int> _items;

            public WriteSetJob(NativeParallelHashSet<int> set, NativeArray<int> items)
            {
                _set = set;
                _items = items;
            }

            public void Execute()
            {
                for (var i = 0; i < _items.Length; i++)
                    _set.Add(_items[i]);
            }
        }
        #endregion

        [Test]
        public void CopyToList_CopiesAllElements()
        {
            var set = new NativeParallelHashSet<int>(3, Allocator.Persistent);
            set.Add(1);
            set.Add(2);
            set.Add(3);
            set.Add(4);
            set.Add(5);

            var output = new NativeList<int>(Allocator.Persistent);

            set.CopyToList(output, default).Complete();

            AssertContainsExactly(ToArray(output), new[] { 1, 2, 3, 4, 5 });

            output.Dispose();
            set.Dispose();
        }

        [Test]
        public void CopyToList_AppendsToExistingOutput()
        {
            var set = new NativeParallelHashSet<int>(3, Allocator.Persistent);
            set.Add(1);
            set.Add(2);

            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(9);
            output.Add(8);

            set.CopyToList(output, default).Complete();

            Assert.AreEqual(4, output.Length);
            Assert.AreEqual(9, output[0]);
            Assert.AreEqual(8, output[1]);

            var appended = new[] { output[2], output[3] };
            AssertContainsExactly(appended, new[] { 1, 2 });

            output.Dispose();
            set.Dispose();
        }

        [Test]
        public void CopyToList_EmptySet_LeavesOutputUnchanged()
        {
            var set = new NativeParallelHashSet<int>(3, Allocator.Persistent);

            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(5);

            set.CopyToList(output, default).Complete();

            AssertContainsExactly(ToArray(output), new[] { 5 });

            output.Dispose();
            set.Dispose();
        }

        [Test]
        public void CopyToList_LargeSet()
        {
            var count = 10000;
            var set = new NativeParallelHashSet<int>(count, Allocator.Persistent);
            var expected = new int[count];
            for (var i = 0; i < count; i++)
            {
                set.Add(i);
                expected[i] = i;
            }

            var output = new NativeList<int>(Allocator.Persistent);

            set.CopyToList(output, default).Complete();

            AssertContainsExactly(ToArray(output), expected);

            output.Dispose();
            set.Dispose();
        }

        [Test]
        public void CopyToList_WaitsForDependentJob()
        {
            var set = new NativeParallelHashSet<int>(3, Allocator.Persistent);
            var items = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);

            var writeJob = new WriteSetJob(set, items).Schedule();
            var copyJob = set.CopyToList(output, writeJob);

            copyJob.Complete();

            AssertContainsExactly(ToArray(output), new[] { 1, 2, 3 });

            output.Dispose();
            items.Dispose();
            set.Dispose();
        }
    }
}
