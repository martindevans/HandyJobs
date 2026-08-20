using NUnit.Framework;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;

namespace Tests.Collections
{
    public class JobFillExtensionsTests
    {
        #region NativeArray

        [Test]
        public void Array_FillsWithDefault()
        {
            var array = new NativeArray<int>(5, Allocator.Persistent);

            array.Fill(default).Complete();

            for (var i = 0; i < array.Length; i++)
                Assert.AreEqual(0, array[i], $"Expected 0 at index {i}");

            array.Dispose();
        }

        [Test]
        public void Array_FillsWithGivenValue()
        {
            var array = new NativeArray<int>(5, Allocator.Persistent);

            array.Fill(default, 42).Complete();

            for (var i = 0; i < array.Length; i++)
                Assert.AreEqual(42, array[i], $"Expected 42 at index {i}");

            array.Dispose();
        }

        [Test]
        public void Array_FillsWithNonTrivialValueType()
        {
            var array = new NativeArray<float>(3, Allocator.Persistent);

            array.Fill(default, 1.5f).Complete();

            for (var i = 0; i < array.Length; i++)
                Assert.AreEqual(1.5f, array[i], $"Expected 1.5 at index {i}");

            array.Dispose();
        }

        [Test]
        public void Array_FillsEmptyArray()
        {
            var array = new NativeArray<int>(0, Allocator.Persistent);

            array.Fill(default, 1).Complete();

            Assert.AreEqual(0, array.Length);

            array.Dispose();
        }

        [Test]
        public void Array_FillsLargeArray()
        {
            var array = new NativeArray<int>(10000, Allocator.Persistent);

            array.Fill(default, -7).Complete();

            for (var i = 0; i < array.Length; i++)
                Assert.AreEqual(-7, array[i], $"Expected -7 at index {i}");

            array.Dispose();
        }

        [Test]
        public void Array_OverwritesExistingValues()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Persistent);

            array.Fill(default, 9).Complete();

            for (var i = 0; i < array.Length; i++)
                Assert.AreEqual(9, array[i], $"Expected 9 at index {i}");

            array.Dispose();
        }

        [Test]
        public void Array_WaitsForDependentJob()
        {
            var array = new NativeArray<int>(100, Allocator.Persistent);

            var sentinelJob = array.Fill(default, -1);
            var fillJob = array.Fill(sentinelJob, 5);

            fillJob.Complete();

            for (var i = 0; i < array.Length; i++)
                Assert.AreEqual(5, array[i], $"Expected 5 at index {i}");

            array.Dispose();
        }

        #endregion

        #region NativeSlice

        [Test]
        public void Slice_FillsOnlySlice()
        {
            var array = new NativeArray<int>(5, Allocator.Persistent);

            var slice = array.Slice(1, 3);
            slice.Fill(default, 7).Complete();

            Assert.AreEqual(0, array[0]);
            Assert.AreEqual(7, array[1]);
            Assert.AreEqual(7, array[2]);
            Assert.AreEqual(7, array[3]);
            Assert.AreEqual(0, array[4]);

            array.Dispose();
        }

        [Test]
        public void Slice_FillsWithDefault()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            var slice = array.Slice(1, 2);
            slice.Fill(default).Complete();

            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(0, array[1]);
            Assert.AreEqual(0, array[2]);
            Assert.AreEqual(4, array[3]);

            array.Dispose();
        }

        [Test]
        public void Slice_FillsWholeArray()
        {
            var array = new NativeArray<int>(4, Allocator.Persistent);

            var slice = array.Slice(0, array.Length);
            slice.Fill(default, 3).Complete();

            for (var i = 0; i < array.Length; i++)
                Assert.AreEqual(3, array[i], $"Expected 3 at index {i}");

            array.Dispose();
        }

        [Test]
        public void Slice_FillsEmptySlice()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);

            var slice = array.Slice(1, 0);
            slice.Fill(default, 9).Complete();

            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(2, array[1]);
            Assert.AreEqual(3, array[2]);

            array.Dispose();
        }

        [Test]
        public void Slice_FillsLargeSlice()
        {
            var array = new NativeArray<int>(10000, Allocator.Persistent);

            var slice = array.Slice(2500, 5000);
            slice.Fill(default, 11).Complete();

            Assert.AreEqual(0, array[2499]);
            for (var i = 2500; i < 7500; i++)
                Assert.AreEqual(11, array[i], $"Expected 11 at index {i}");
            Assert.AreEqual(0, array[7500]);

            array.Dispose();
        }

        [Test]
        public void Slice_WaitsForDependentJob()
        {
            var array = new NativeArray<int>(100, Allocator.Persistent);

            var sentinelJob = array.Fill(default, -1);
            var slice = array.Slice(25, 50);
            var fillJob = slice.Fill(sentinelJob, 5);

            fillJob.Complete();

            for (var i = 25; i < 75; i++)
                Assert.AreEqual(5, array[i], $"Expected 5 at index {i}");

            array.Dispose();
        }

        #endregion
    }
}
