using System;
using System.Collections.Generic;
using NUnit.Framework;
using Packages.HandyJobs.Runtime.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace HandyJobs.Tests.Collections
{
    public class JobKeyValuePairArrayExtensionsTests
    {
        #region Helper Methods
        private static NativeArray<KeyValuePair<int, int>> CreatePairs(int[] keys, int[] values)
        {
            var pairs = new NativeArray<KeyValuePair<int, int>>(keys.Length, Allocator.Persistent);
            for (var i = 0; i < keys.Length; i++)
                pairs[i] = new KeyValuePair<int, int>(keys[i], values[i]);
            return pairs;
        }

        private static int[] ToArray(NativeList<int> list)
        {
            var result = new int[list.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = list[i];
            return result;
        }

        private struct WritePairsJob : IJob
        {
            private NativeArray<KeyValuePair<int, int>> _pairs;
            private NativeArray<int> _keys;
            private NativeArray<int> _values;

            public WritePairsJob(NativeArray<KeyValuePair<int, int>> pairs, NativeArray<int> keys, NativeArray<int> values)
            {
                _pairs = pairs;
                _keys = keys;
                _values = values;
            }

            public void Execute()
            {
                for (var i = 0; i < _pairs.Length; i++)
                    _pairs[i] = new KeyValuePair<int, int>(_keys[i], _values[i]);
            }
        }
        #endregion

        #region CopyKeys

        [Test]
        public void CopyKeys_CopiesKeysInOrder()
        {
            var pairs = CreatePairs(new[] { 1, 2, 3 }, new[] { 100, 200, 300 });
            var output = new NativeList<int>(Allocator.Persistent);

            pairs.Slice().CopyKeys(output).Complete();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ToArray(output));

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyKeys_AppendsToExistingOutput()
        {
            var pairs = CreatePairs(new[] { 1, 2 }, new[] { 10, 20 });
            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(9);
            output.Add(8);

            pairs.Slice().CopyKeys(output).Complete();

            CollectionAssert.AreEqual(new[] { 9, 8, 1, 2 }, ToArray(output));

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyKeys_EmptyInput_LeavesOutputUnchanged()
        {
            var pairs = CreatePairs(Array.Empty<int>(), Array.Empty<int>());
            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(5);

            pairs.Slice().CopyKeys(output).Complete();

            CollectionAssert.AreEqual(new[] { 5 }, ToArray(output));

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyKeys_LargeInput()
        {
            var count = 10000;
            var keys = new int[count];
            var values = new int[count];
            for (var i = 0; i < count; i++)
            {
                keys[i] = i;
                values[i] = i * 2;
            }

            var pairs = CreatePairs(keys, values);
            var output = new NativeList<int>(Allocator.Persistent);

            pairs.Slice().CopyKeys(output).Complete();

            Assert.AreEqual(count, output.Length);
            for (var i = 0; i < count; i++)
                Assert.AreEqual(i, output[i], $"Expected key {i} at index {i}");

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyKeys_WaitsForDependentJob()
        {
            var pairs = new NativeArray<KeyValuePair<int, int>>(3, Allocator.Persistent);
            var keys = new NativeArray<int>(new[] { 7, 8, 9 }, Allocator.Persistent);
            var values = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);

            var writeJob = new WritePairsJob(pairs, keys, values).Schedule();
            var copyJob = pairs.Slice().CopyKeys(output, writeJob);

            copyJob.Complete();

            CollectionAssert.AreEqual(new[] { 7, 8, 9 }, ToArray(output));

            output.Dispose();
            values.Dispose();
            keys.Dispose();
            pairs.Dispose();
        }

        #endregion

        #region CopyValue

        [Test]
        public void CopyValue_CopiesValuesInOrder()
        {
            var pairs = CreatePairs(new[] { 1, 2, 3 }, new[] { 100, 200, 300 });
            var output = new NativeList<int>(Allocator.Persistent);

            pairs.Slice().CopyValue(output).Complete();

            CollectionAssert.AreEqual(new[] { 100, 200, 300 }, ToArray(output));

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyValue_AppendsToExistingOutput()
        {
            var pairs = CreatePairs(new[] { 1, 2 }, new[] { 10, 20 });
            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(9);
            output.Add(8);

            pairs.Slice().CopyValue(output).Complete();

            CollectionAssert.AreEqual(new[] { 9, 8, 10, 20 }, ToArray(output));

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyValue_EmptyInput_LeavesOutputUnchanged()
        {
            var pairs = CreatePairs(Array.Empty<int>(), Array.Empty<int>());
            var output = new NativeList<int>(Allocator.Persistent);
            output.Add(5);

            pairs.Slice().CopyValue(output).Complete();

            CollectionAssert.AreEqual(new[] { 5 }, ToArray(output));

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyValue_LargeInput()
        {
            var count = 10000;
            var keys = new int[count];
            var values = new int[count];
            for (var i = 0; i < count; i++)
            {
                keys[i] = i;
                values[i] = i * 3;
            }

            var pairs = CreatePairs(keys, values);
            var output = new NativeList<int>(Allocator.Persistent);

            pairs.Slice().CopyValue(output).Complete();

            Assert.AreEqual(count, output.Length);
            for (var i = 0; i < count; i++)
                Assert.AreEqual(i * 3, output[i], $"Expected value {i * 3} at index {i}");

            output.Dispose();
            pairs.Dispose();
        }

        [Test]
        public void CopyValue_WaitsForDependentJob()
        {
            var pairs = new NativeArray<KeyValuePair<int, int>>(3, Allocator.Persistent);
            var keys = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var values = new NativeArray<int>(new[] { 7, 8, 9 }, Allocator.Persistent);
            var output = new NativeList<int>(Allocator.Persistent);

            var writeJob = new WritePairsJob(pairs, keys, values).Schedule();
            var copyJob = pairs.Slice().CopyValue(output, writeJob);

            copyJob.Complete();

            CollectionAssert.AreEqual(new[] { 7, 8, 9 }, ToArray(output));

            output.Dispose();
            values.Dispose();
            keys.Dispose();
            pairs.Dispose();
        }

        #endregion
    }
}
