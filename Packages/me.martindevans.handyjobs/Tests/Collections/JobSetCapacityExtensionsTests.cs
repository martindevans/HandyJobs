using NUnit.Framework;
using System.Collections.Generic;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Tests.Collections
{
    public class JobSetCapacityExtensionsTests
    {
        #region Helper Methods
        private static int[] ToArray(NativeParallelHashSet<int> set)
        {
            var list = new List<int>();
            foreach (var item in set)
                list.Add(item);
            list.Sort();
            return list.ToArray();
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
                    _list.Add(i);
            }
        }
        #endregion

        #region NativeList Source

        [Test]
        public void NativeList_GrowsCapacityToMatchListLength()
        {
            var set = new NativeParallelHashSet<int>(1, Allocator.Persistent);
            var before = set.Capacity;

            var list = new NativeList<int>(10, Allocator.Persistent);
            for (var i = 0; i < 10; i++)
                list.Add(i);

            set.SetCapacity(list, default).Complete();

            Assert.GreaterOrEqual(set.Capacity, 10);
            Assert.Greater(set.Capacity, before);

            list.Dispose();
            set.Dispose();
        }

        [Test]
        public void NativeList_FactorScalesCapacity()
        {
            var set = new NativeParallelHashSet<int>(1, Allocator.Persistent);

            var list = new NativeList<int>(10, Allocator.Persistent);
            for (var i = 0; i < 10; i++)
                list.Add(i);

            set.SetCapacity(list, 2f, default).Complete();

            Assert.GreaterOrEqual(set.Capacity, 20);

            list.Dispose();
            set.Dispose();
        }

        [Test]
        public void NativeList_NeverShrinksCapacity()
        {
            var set = new NativeParallelHashSet<int>(100, Allocator.Persistent);
            var before = set.Capacity;

            var list = new NativeList<int>(2, Allocator.Persistent);
            list.Add(0);
            list.Add(1);

            set.SetCapacity(list, default).Complete();

            Assert.AreEqual(before, set.Capacity);

            list.Dispose();
            set.Dispose();
        }

        [Test]
        public void NativeList_EmptyList_DoesNotShrinkCapacity()
        {
            var set = new NativeParallelHashSet<int>(10, Allocator.Persistent);
            var before = set.Capacity;

            var list = new NativeList<int>(0, Allocator.Persistent);

            set.SetCapacity(list, default).Complete();

            Assert.AreEqual(before, set.Capacity);

            list.Dispose();
            set.Dispose();
        }

        [Test]
        public void NativeList_PreservesContents()
        {
            var set = new NativeParallelHashSet<int>(1, Allocator.Persistent);
            set.Add(1);
            set.Add(2);
            set.Add(3);

            var list = new NativeList<int>(10, Allocator.Persistent);
            for (var i = 0; i < 10; i++)
                list.Add(i);

            set.SetCapacity(list, default).Complete();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ToArray(set));

            list.Dispose();
            set.Dispose();
        }

        [Test]
        public void NativeList_WaitsForDependentJob()
        {
            var set = new NativeParallelHashSet<int>(1, Allocator.Persistent);
            var list = new NativeList<int>(Allocator.Persistent);

            var writeJob = new WriteListJob(list, 20).Schedule();
            var capacityJob = set.SetCapacity(list, writeJob);

            capacityJob.Complete();

            Assert.GreaterOrEqual(set.Capacity, 20);

            list.Dispose();
            set.Dispose();
        }

        #endregion
    }
}
