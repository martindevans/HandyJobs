using NUnit.Framework;
using Packages.HandyJobs.Runtime.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace HandyJobs.Tests.Collections
{
    public class JobReverseExtensionsTests
    {
        #region Helper Methods
        private static int[] Reversed(int[] original)
        {
            var result = new int[original.Length];
            for (var i = 0; i < original.Length; i++)
                result[i] = original[original.Length - 1 - i];
            return result;
        }

        private static int[] ToArray(NativeArray<int> array)
        {
            var result = new int[array.Length];
            for (var i = 0; i < array.Length; i++)
                result[i] = array[i];
            return result;
        }

        private static int[] ToArray(NativeList<int> list)
        {
            var result = new int[list.Length];
            for (var i = 0; i < list.Length; i++)
                result[i] = list[i];
            return result;
        }

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
                    _array[i] = i + 1;
            }
        }

        private struct WriteListJob : IJob
        {
            private NativeList<int> _list;

            public WriteListJob(NativeList<int> list)
            {
                _list = list;
            }

            public void Execute()
            {
                for (var i = 0; i < 5; i++)
                    _list.Add(i + 1);
            }
        }
        #endregion

        #region NativeArray

        [Test]
        public void Array_ReversesElements()
        {
            var original = new[] { 1, 2, 3, 4, 5 };
            var array = new NativeArray<int>(original, Allocator.Persistent);

            array.Reverse(default).Complete();

            CollectionAssert.AreEqual(Reversed(original), ToArray(array));

            array.Dispose();
        }

        [Test]
        public void Array_ReversesEvenLength()
        {
            var original = new[] { 1, 2, 3, 4 };
            var array = new NativeArray<int>(original, Allocator.Persistent);

            array.Reverse(default).Complete();

            CollectionAssert.AreEqual(Reversed(original), ToArray(array));

            array.Dispose();
        }

        [Test]
        public void Array_SingleElement_Unchanged()
        {
            var array = new NativeArray<int>(new[] { 42 }, Allocator.Persistent);

            array.Reverse(default).Complete();

            CollectionAssert.AreEqual(new[] { 42 }, ToArray(array));

            array.Dispose();
        }

        [Test]
        public void Array_Palindrome_Unchanged()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 2, 1 }, Allocator.Persistent);

            array.Reverse(default).Complete();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 2, 1 }, ToArray(array));

            array.Dispose();
        }

        [Test]
        public void Array_EmptyArray()
        {
            var array = new NativeArray<int>(0, Allocator.Persistent);

            array.Reverse(default).Complete();

            Assert.AreEqual(0, array.Length);

            array.Dispose();
        }

        [Test]
        public void Array_LargeArray()
        {
            var count = 10000;
            var original = new int[count];
            for (var i = 0; i < count; i++)
                original[i] = i;

            var array = new NativeArray<int>(original, Allocator.Persistent);

            array.Reverse(default).Complete();

            CollectionAssert.AreEqual(Reversed(original), ToArray(array));

            array.Dispose();
        }

        [Test]
        public void Array_WaitsForDependentJob()
        {
            var array = new NativeArray<int>(5, Allocator.Persistent);

            var writeJob = new WriteArrayJob(array).Schedule();
            var reverseJob = array.Reverse(writeJob);

            reverseJob.Complete();

            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, ToArray(array));

            array.Dispose();
        }

        #endregion

        #region NativeList

        [Test]
        public void List_ReversesElements()
        {
            var list = new NativeList<int>(Allocator.Persistent);
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);

            list.Reverse(default).Complete();

            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, ToArray(list));

            list.Dispose();
        }

        [Test]
        public void List_SingleElement_Unchanged()
        {
            var list = new NativeList<int>(Allocator.Persistent);
            list.Add(42);

            list.Reverse(default).Complete();

            CollectionAssert.AreEqual(new[] { 42 }, ToArray(list));

            list.Dispose();
        }

        [Test]
        public void List_EmptyList()
        {
            var list = new NativeList<int>(Allocator.Persistent);

            list.Reverse(default).Complete();

            Assert.AreEqual(0, list.Length);

            list.Dispose();
        }

        [Test]
        public void List_LargeList()
        {
            var count = 10000;
            var list = new NativeList<int>(count, Allocator.Persistent);
            var original = new int[count];
            for (var i = 0; i < count; i++)
            {
                list.Add(i);
                original[i] = i;
            }

            list.Reverse(default).Complete();

            CollectionAssert.AreEqual(Reversed(original), ToArray(list));

            list.Dispose();
        }

        [Test]
        public void List_WaitsForDependentJob()
        {
            var list = new NativeList<int>(Allocator.Persistent);

            var writeJob = new WriteListJob(list).Schedule();
            var reverseJob = list.Reverse(writeJob);

            reverseJob.Complete();

            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, ToArray(list));

            list.Dispose();
        }

        #endregion
    }
}
