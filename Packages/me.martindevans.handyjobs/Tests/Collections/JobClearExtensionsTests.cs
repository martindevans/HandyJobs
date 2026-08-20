using NUnit.Framework;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Tests.Collections
{
    public class JobClearExtensionsTests
    {
        #region Helper Methods
        private struct FillListJob : IJob
        {
            private NativeList<int> _list;
            private int _count;

            public FillListJob(NativeList<int> list, int count)
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

        private struct FillSetJob : IJob
        {
            private NativeHashSet<int> _set;
            private int _count;

            public FillSetJob(NativeHashSet<int> set, int count)
            {
                _set = set;
                _count = count;
            }

            public void Execute()
            {
                for (var i = 0; i < _count; i++)
                    _set.Add(i);
            }
        }
        #endregion

        #region NativeList

        [Test]
        public void List_ClearsElements()
        {
            var list = new NativeList<int>(Allocator.Persistent);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            list.Clear(default).Complete();

            Assert.AreEqual(0, list.Length);

            list.Dispose();
        }

        [Test]
        public void List_ClearsEmptyList()
        {
            var list = new NativeList<int>(Allocator.Persistent);

            list.Clear(default).Complete();

            Assert.AreEqual(0, list.Length);

            list.Dispose();
        }

        [Test]
        public void List_ClearsLargeList()
        {
            var list = new NativeList<int>(10000, Allocator.Persistent);
            for (var i = 0; i < 10000; i++)
                list.Add(i);

            list.Clear(default).Complete();

            Assert.AreEqual(0, list.Length);

            list.Dispose();
        }

        [Test]
        public void List_CanBeReusedAfterClear()
        {
            var list = new NativeList<int>(Allocator.Persistent);
            list.Add(1);

            list.Clear(default).Complete();

            list.Add(2);
            list.Add(3);

            Assert.AreEqual(2, list.Length);
            Assert.AreEqual(2, list[0]);
            Assert.AreEqual(3, list[1]);

            list.Dispose();
        }

        [Test]
        public void List_WaitsForDependentJob()
        {
            var list = new NativeList<int>(Allocator.Persistent);

            var fillJob = new FillListJob(list, 100).Schedule();
            var clearJob = list.Clear(fillJob);

            clearJob.Complete();

            Assert.AreEqual(0, list.Length);

            list.Dispose();
        }

        #endregion

        #region NativeHashSet

        [Test]
        public void HashSet_ClearsElements()
        {
            var set = new NativeHashSet<int>(128, Allocator.Persistent);
            set.Add(1);
            set.Add(2);
            set.Add(2);
            set.Add(3);

            set.Clear(default).Complete();

            Assert.AreEqual(0, set.Count);

            set.Dispose();
        }

        [Test]
        public void HashSet_ClearsEmptySet()
        {
            var set = new NativeHashSet<int>(128, Allocator.Persistent);

            set.Clear(default).Complete();

            Assert.AreEqual(0, set.Count);

            set.Dispose();
        }

        [Test]
        public void HashSet_ClearsLargeSet()
        {
            var set = new NativeHashSet<int>(10000, Allocator.Persistent);
            for (var i = 0; i < 10000; i++)
                set.Add(i);

            set.Clear(default).Complete();

            Assert.AreEqual(0, set.Count);

            set.Dispose();
        }

        [Test]
        public void HashSet_CanBeReusedAfterClear()
        {
            var set = new NativeHashSet<int>(128, Allocator.Persistent);
            set.Add(1);

            set.Clear(default).Complete();

            set.Add(2);
            set.Add(3);

            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains(2));
            Assert.IsTrue(set.Contains(3));

            set.Dispose();
        }

        [Test]
        public void HashSet_WaitsForDependentJob()
        {
            var set = new NativeHashSet<int>(128, Allocator.Persistent);

            var fillJob = new FillSetJob(set, 100).Schedule();
            var clearJob = set.Clear(fillJob);

            clearJob.Complete();

            Assert.AreEqual(0, set.Count);

            set.Dispose();
        }

        #endregion

        #region NativeQueue

        [Test]
        public void Queue_ClearsElements()
        {
            var queue = new NativeQueue<int>(Allocator.Persistent);
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            queue.Clear(default).Complete();

            Assert.AreEqual(0, queue.Count);

            queue.Dispose();
        }

        [Test]
        public void Queue_ClearsEmptyQueue()
        {
            var queue = new NativeQueue<int>(Allocator.Persistent);

            queue.Clear(default).Complete();

            Assert.AreEqual(0, queue.Count);

            queue.Dispose();
        }

        [Test]
        public void Queue_ClearsLargeQueue()
        {
            var queue = new NativeQueue<int>(Allocator.Persistent);
            for (var i = 0; i < 10000; i++)
                queue.Enqueue(i);

            queue.Clear(default).Complete();

            Assert.AreEqual(0, queue.Count);

            queue.Dispose();
        }

        [Test]
        public void Queue_CanBeReusedAfterClear()
        {
            var queue = new NativeQueue<int>(Allocator.Persistent);
            queue.Enqueue(1);

            queue.Clear(default).Complete();

            queue.Enqueue(2);
            queue.Enqueue(3);

            Assert.AreEqual(2, queue.Count);
            Assert.AreEqual(2, queue.Dequeue());
            Assert.AreEqual(3, queue.Dequeue());

            queue.Dispose();
        }

        #endregion

        #region NativeParallelHashSet

        [Test]
        public void ParallelHashSet_ClearsElements()
        {
            var set = new NativeParallelHashSet<int>(128, Allocator.Persistent);
            set.Add(1);
            set.Add(2);
            set.Add(2);
            set.Add(3);

            set.Clear(default).Complete();

            Assert.AreEqual(0, set.Count());

            set.Dispose();
        }

        [Test]
        public void ParallelHashSet_ClearsEmptySet()
        {
            var set = new NativeParallelHashSet<int>(128, Allocator.Persistent);

            set.Clear(default).Complete();

            Assert.AreEqual(0, set.Count());

            set.Dispose();
        }

        [Test]
        public void ParallelHashSet_ClearsLargeSet()
        {
            var set = new NativeParallelHashSet<int>(10000, Allocator.Persistent);
            for (var i = 0; i < 10000; i++)
                set.Add(i);

            set.Clear(default).Complete();

            Assert.AreEqual(0, set.Count());

            set.Dispose();
        }

        #endregion

        #region NativeHashMap

        [Test]
        public void HashMap_ClearsElements()
        {
            var map = new NativeHashMap<int, int>(128, Allocator.Persistent);
            map.TryAdd(1, 10);
            map.TryAdd(2, 20);
            map.TryAdd(3, 30);

            map.Clear(default).Complete();

            Assert.AreEqual(0, map.Count);

            map.Dispose();
        }

        [Test]
        public void HashMap_ClearsEmptyMap()
        {
            var map = new NativeHashMap<int, int>(128, Allocator.Persistent);

            map.Clear(default).Complete();

            Assert.AreEqual(0, map.Count);

            map.Dispose();
        }

        [Test]
        public void HashMap_ClearsLargeMap()
        {
            var map = new NativeHashMap<int, int>(10000, Allocator.Persistent);
            for (var i = 0; i < 10000; i++)
                map.TryAdd(i, i * 2);

            map.Clear(default).Complete();

            Assert.AreEqual(0, map.Count);

            map.Dispose();
        }

        [Test]
        public void HashMap_CanBeReusedAfterClear()
        {
            var map = new NativeHashMap<int, int>(128, Allocator.Persistent);
            map.TryAdd(1, 10);

            map.Clear(default).Complete();

            map.TryAdd(2, 20);
            map.TryAdd(3, 30);

            Assert.AreEqual(2, map.Count);
            Assert.IsTrue(map.TryGetValue(2, out var value));
            Assert.AreEqual(20, value);

            map.Dispose();
        }

        #endregion

        #region NativeParallelHashMap

        [Test]
        public void ParallelHashMap_ClearsElements()
        {
            var map = new NativeParallelHashMap<int, int>(128, Allocator.Persistent);
            map.TryAdd(1, 10);
            map.TryAdd(2, 20);
            map.TryAdd(3, 30);

            map.Clear(default).Complete();

            Assert.AreEqual(0, map.Count());

            map.Dispose();
        }

        [Test]
        public void ParallelHashMap_ClearsEmptyMap()
        {
            var map = new NativeParallelHashMap<int, int>(128, Allocator.Persistent);

            map.Clear(default).Complete();

            Assert.AreEqual(0, map.Count());

            map.Dispose();
        }

        [Test]
        public void ParallelHashMap_ClearsLargeMap()
        {
            var map = new NativeParallelHashMap<int, int>(10000, Allocator.Persistent);
            for (var i = 0; i < 10000; i++)
                map.TryAdd(i, i * 2);

            map.Clear(default).Complete();

            Assert.AreEqual(0, map.Count());

            map.Dispose();
        }

        #endregion
    }
}
