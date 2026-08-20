using NUnit.Framework;
using me.martindevans.handyjobs.Primitives.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Tests.Collections
{
    public class JobFillRandomExtensionsTests
    {
        private const uint Seed = 12345;

        #region Helper Methods
        private static void AssertArraysEqual<T>(NativeArray<T> a, NativeArray<T> b)
            where T : struct
        {
            AssertArraysEqual(a.Slice(), b.Slice());
        }

        private static void AssertArraysEqual<T>(NativeSlice<T> a, NativeSlice<T> b)
            where T : struct
        {
            Assert.AreEqual(a.Length, b.Length);
            for (var i = 0; i < a.Length; i++)
                Assert.AreEqual(a[i], b[i], $"Mismatch at index {i}");
        }

        private static void AssertAllWithinRange(NativeArray<double> array, double min, double max)
        {
            AssertAllWithinRange(array.Slice(), min, max);
        }

        private static void AssertAllWithinRange(NativeSlice<double> array, double min, double max)
        {
            for (var i = 0; i < array.Length; i++)
                Assert.That(array[i], Is.GreaterThanOrEqualTo(min).And.LessThanOrEqualTo(max),
                    $"Value {array[i]} at index {i} outside range [{min}, {max}]");
        }

        private static void AssertAllWithinRange(NativeArray<float> array, float min, float max)
        {
            AssertAllWithinRange(array.Slice(), min, max);
        }

        private static void AssertAllWithinRange(NativeSlice<float> array, float min, float max)
        {
            for (var i = 0; i < array.Length; i++)
                Assert.That(array[i], Is.GreaterThanOrEqualTo(min).And.LessThanOrEqualTo(max),
                    $"Value {array[i]} at index {i} outside range [{min}, {max}]");
        }

        private static void AssertAllWithinRange(NativeArray<int> array, int min, int max)
        {
            AssertAllWithinRange(array.Slice(), min, max);
        }

        private static void AssertAllWithinRange(NativeSlice<int> array, int min, int max)
        {
            for (var i = 0; i < array.Length; i++)
                Assert.That(array[i], Is.GreaterThanOrEqualTo(min).And.LessThan(max),
                    $"Value {array[i]} at index {i} outside range [{min}, {max})");
        }

        private static int CountDifferences<T>(NativeArray<T> a, NativeArray<T> b)
            where T : struct
        {
            var count = 0;
            for (var i = 0; i < a.Length; i++)
                if (!a[i].Equals(b[i])) count++;
            return count;
        }
        #endregion

        #region double

        [Test]
        public void Double_Array_AllValuesWithinRange()
        {
            var array = new NativeArray<double>(1000, Allocator.Persistent);

            array.FillRandom(Seed, -10.0, 10.0).Complete();

            AssertAllWithinRange(array, -10.0, 10.0);

            array.Dispose();
        }

        [Test]
        public void Double_Array_SameSeedProducesSameValues()
        {
            var a = new NativeArray<double>(500, Allocator.Persistent);
            var b = new NativeArray<double>(500, Allocator.Persistent);

            a.FillRandom(Seed, 0.0, 1.0).Complete();
            b.FillRandom(Seed, 0.0, 1.0).Complete();

            AssertArraysEqual(a, b);

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Double_Array_DifferentSeedsProduceDifferentValues()
        {
            var a = new NativeArray<double>(1000, Allocator.Persistent);
            var b = new NativeArray<double>(1000, Allocator.Persistent);

            a.FillRandom(1, 0.0, 1.0).Complete();
            b.FillRandom(2, 0.0, 1.0).Complete();

            Assert.Greater(CountDifferences(a, b), 0, "Different seeds should produce different values");

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Double_Array_FillsEmptyArray()
        {
            var array = new NativeArray<double>(0, Allocator.Persistent);

            array.FillRandom(Seed, 0.0, 1.0).Complete();

            Assert.AreEqual(0, array.Length);

            array.Dispose();
        }

        [Test]
        public void Double_Array_FillsSingleElement()
        {
            var array = new NativeArray<double>(1, Allocator.Persistent);

            array.FillRandom(Seed, 2.0, 3.0).Complete();

            AssertAllWithinRange(array, 2.0, 3.0);

            array.Dispose();
        }

        [Test]
        public void Double_Slice_FillsOnlySlice()
        {
            var array = new NativeArray<double>(new[] { -999.0, -999.0, -999.0, -999.0, -999.0 }, Allocator.Persistent);

            var slice = array.Slice(1, 3);
            slice.FillRandom(Seed, 0.0, 1.0).Complete();

            Assert.AreEqual(-999.0, array[0]);
            AssertAllWithinRange(array.Slice(1, 3), 0.0, 1.0);
            Assert.AreEqual(-999.0, array[4]);

            array.Dispose();
        }

        [Test]
        public void Double_Slice_SameSeedProducesSameValues()
        {
            var a = new NativeArray<double>(100, Allocator.Persistent);
            var b = new NativeArray<double>(100, Allocator.Persistent);

            a.Slice(10, 50).FillRandom(Seed, 0.0, 1.0).Complete();
            b.Slice(10, 50).FillRandom(Seed, 0.0, 1.0).Complete();

            AssertArraysEqual(a.Slice(10, 50), b.Slice(10, 50));

            a.Dispose();
            b.Dispose();
        }

        #endregion

        #region float

        [Test]
        public void Float_Array_AllValuesWithinRange()
        {
            var array = new NativeArray<float>(1000, Allocator.Persistent);

            array.FillRandom(Seed, -5f, 5f).Complete();

            AssertAllWithinRange(array, -5f, 5f);

            array.Dispose();
        }

        [Test]
        public void Float_Array_SameSeedProducesSameValues()
        {
            var a = new NativeArray<float>(1000, Allocator.Persistent);
            var b = new NativeArray<float>(1000, Allocator.Persistent);

            a.FillRandom(Seed, 0f, 1f).Complete();
            b.FillRandom(Seed, 0f, 1f).Complete();

            AssertArraysEqual(a, b);

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Float_Array_DifferentSeedsProduceDifferentValues()
        {
            var a = new NativeArray<float>(1000, Allocator.Persistent);
            var b = new NativeArray<float>(1000, Allocator.Persistent);

            a.FillRandom(1, 0f, 1f).Complete();
            b.FillRandom(2, 0f, 1f).Complete();

            Assert.Greater(CountDifferences(a, b), 0, "Different seeds should produce different values");

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Float_Array_ArrayAndSliceOverloadsAgree()
        {
            var a = new NativeArray<float>(200, Allocator.Persistent);
            var b = new NativeArray<float>(200, Allocator.Persistent);

            a.FillRandom(Seed, 0f, 10f).Complete();
            b.Slice(0, b.Length).FillRandom(Seed, 0f, 10f).Complete();

            AssertArraysEqual(a, b);

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Float_Array_FirstBatchUnaffectedByTotalLength()
        {
            var exact = new NativeArray<float>(64, Allocator.Persistent);
            var longer = new NativeArray<float>(65, Allocator.Persistent);

            exact.FillRandom(Seed, 0f, 1f).Complete();
            longer.FillRandom(Seed, 0f, 1f).Complete();

            for (var i = 0; i < exact.Length; i++)
                Assert.AreEqual(exact[i], longer[i], $"Index {i} differs between batch-aligned and longer array");

            exact.Dispose();
            longer.Dispose();
        }

        [Test]
        public void Float_Array_FillsEmptyArray()
        {
            var array = new NativeArray<float>(0, Allocator.Persistent);

            array.FillRandom(Seed, 0f, 1f).Complete();

            Assert.AreEqual(0, array.Length);

            array.Dispose();
        }

        [Test]
        public void Float_Slice_FillsOnlySlice()
        {
            var array = new NativeArray<float>(new[] { -999f, -999f, -999f, -999f, -999f }, Allocator.Persistent);

            var slice = array.Slice(1, 3);
            slice.FillRandom(Seed, 0f, 1f).Complete();

            Assert.AreEqual(-999f, array[0]);
            AssertAllWithinRange(array.Slice(1, 3), 0f, 1f);
            Assert.AreEqual(-999f, array[4]);

            array.Dispose();
        }

        #endregion

        #region int

        [Test]
        public void Int_Array_AllValuesWithinRange()
        {
            var array = new NativeArray<int>(1000, Allocator.Persistent);

            array.FillRandom(Seed, -100, 100).Complete();

            AssertAllWithinRange(array, -100, 100);

            array.Dispose();
        }

        [Test]
        public void Int_Array_SameSeedProducesSameValues()
        {
            var a = new NativeArray<int>(1000, Allocator.Persistent);
            var b = new NativeArray<int>(1000, Allocator.Persistent);

            a.FillRandom(Seed, 0, 1000).Complete();
            b.FillRandom(Seed, 0, 1000).Complete();

            AssertArraysEqual(a, b);

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Int_Array_DifferentSeedsProduceDifferentValues()
        {
            var a = new NativeArray<int>(1000, Allocator.Persistent);
            var b = new NativeArray<int>(1000, Allocator.Persistent);

            a.FillRandom(1, 0, 1000).Complete();
            b.FillRandom(2, 0, 1000).Complete();

            Assert.Greater(CountDifferences(a, b), 0, "Different seeds should produce different values");

            a.Dispose();
            b.Dispose();
        }

        [Test]
        public void Int_Array_SmallRangeProducesAllValues()
        {
            var array = new NativeArray<int>(1000, Allocator.Persistent);

            array.FillRandom(Seed, 0, 3).Complete();

            var seen = new bool[3];
            for (var i = 0; i < array.Length; i++)
                seen[array[i]] = true;

            for (var v = 0; v < 3; v++)
                Assert.IsTrue(seen[v], $"Value {v} was never produced");

            array.Dispose();
        }

        [Test]
        public void Int_Array_FillsEmptyArray()
        {
            var array = new NativeArray<int>(0, Allocator.Persistent);

            array.FillRandom(Seed, 0, 1).Complete();

            Assert.AreEqual(0, array.Length);

            array.Dispose();
        }

        [Test]
        public void Int_Slice_FillsOnlySlice()
        {
            var array = new NativeArray<int>(new[] { -999, -999, -999, -999, -999 }, Allocator.Persistent);

            var slice = array.Slice(1, 3);
            slice.FillRandom(Seed, 0, 10).Complete();

            Assert.AreEqual(-999, array[0]);
            AssertAllWithinRange(array.Slice(1, 3), 0, 10);
            Assert.AreEqual(-999, array[4]);

            array.Dispose();
        }

        #endregion

        #region Dependencies

        [Test]
        public void WaitsForDependentJob()
        {
            var array = new NativeArray<float>(100, Allocator.Persistent);

            var sentinelJob = new SentinelFillJob(array).Schedule();
            var fillJob = array.FillRandom(Seed, 0f, 1f, sentinelJob);

            fillJob.Complete();

            AssertAllWithinRange(array, 0f, 1f);

            array.Dispose();
        }

        #endregion

        private struct SentinelFillJob
            : IJob
        {
            private NativeArray<float> _array;

            public SentinelFillJob(NativeArray<float> array)
            {
                _array = array;
            }

            public void Execute()
            {
                for (var i = 0; i < _array.Length; i++)
                    _array[i] = -1f;
            }
        }
    }
}
