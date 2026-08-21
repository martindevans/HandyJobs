using NUnit.Framework;
using System;
using me.martindevans.handyjobs.Extensions;

namespace Tests.Extensions
{
    public class SpanExtensionsTests
    {
        #region Helper Methods
        private static int ReferenceSum(int[] values)
        {
            var sum = 0;
            for (var i = 0; i < values.Length; i++)
                sum += values[i];
            return sum;
        }

        private static float ReferenceSum(float[] values)
        {
            var sum = 0f;
            for (var i = 0; i < values.Length; i++)
                sum += values[i];
            return sum;
        }

        private static double ReferenceSum(double[] values)
        {
            var sum = 0.0;
            for (var i = 0; i < values.Length; i++)
                sum += values[i];
            return sum;
        }

        private static float KahanReferenceSum(float[] values)
        {
            var sum = 0f;
            var comp = 0f;
            for (var i = 0; i < values.Length; i++)
            {
                var y = values[i] - comp;
                var t = sum + y;
                comp = (t - sum) - y;
                sum = t;
            }
            return sum;
        }

        private static float VarianceReference(float[] values, float[] probabilities, float mean)
        {
            var sum = 0f;
            var comp = 0f;
            for (var i = 0; i < values.Length; i++)
            {
                var delta = values[i] - mean;
                var v = probabilities[i] * delta * delta;
                var t = sum + v;
                if (Math.Abs(sum) >= Math.Abs(v))
                    comp += (sum - t) + v;
                else
                    comp += (v - t) + sum;
                sum = t;
            }
            return sum + comp;
        }

        private static float SumSqrReferenceSum(float[] values)
        {
            var sum = 0f;
            var comp = 0f;
            for (var i = 0; i < values.Length; i++)
            {
                var v = values[i] * values[i];
                var t = sum + v;
                if (Math.Abs(sum) >= Math.Abs(v))
                    comp += (sum - t) + v;
                else
                    comp += (v - t) + sum;
                sum = t;
            }
            return sum + comp;
        }

        private static void AssertFloatApproximately(float expected, float actual, string context)
        {
            var tolerance = Math.Max(1e-3f, Math.Abs(expected) * 1e-4f);
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance), context);
        }

        private static void AssertDoubleApproximately(double expected, double actual, string context)
        {
            var tolerance = Math.Max(1e-9, Math.Abs(expected) * 1e-12);
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance), context);
        }
        #endregion

        #region AsReadOnlySpan

        [Test]
        public void Array_AsReadOnlySpan_ReturnsAllElements()
        {
            var arr = new[] { 1, 2, 3 };

            var span = arr.AsReadOnlySpan();

            Assert.AreEqual(3, span.Length);
            Assert.AreEqual(1, span[0]);
            Assert.AreEqual(2, span[1]);
            Assert.AreEqual(3, span[2]);
        }

        [Test]
        public void Array_AsReadOnlySpan_Start_ReturnsTail()
        {
            var arr = new[] { 1, 2, 3, 4 };

            var span = arr.AsReadOnlySpan(2);

            Assert.AreEqual(2, span.Length);
            Assert.AreEqual(3, span[0]);
            Assert.AreEqual(4, span[1]);
        }

        [Test]
        public void Array_AsReadOnlySpan_StartAndCount_ReturnsSubspan()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            var span = arr.AsReadOnlySpan(1, 3);

            Assert.AreEqual(3, span.Length);
            Assert.AreEqual(2, span[0]);
            Assert.AreEqual(3, span[1]);
            Assert.AreEqual(4, span[2]);
        }

        [Test]
        public void Array_AsReadOnlySpan_StartAndCount_SingleElement()
        {
            var arr = new[] { 1, 2, 3 };

            var span = arr.AsReadOnlySpan(1, 1);

            Assert.AreEqual(1, span.Length);
            Assert.AreEqual(2, span[0]);
        }

        [Test]
        public void Span_AsReadOnlySpan_ReturnsSameElements()
        {
            var arr = new[] { 7, 8, 9, 10 };

            var span = arr.AsSpan(1, 2).AsReadOnlySpan();

            Assert.AreEqual(2, span.Length);
            Assert.AreEqual(8, span[0]);
            Assert.AreEqual(9, span[1]);
        }

        #endregion

        #region Sum

        [Test]
        public void Sum_EmptySpan_ReturnsZero()
        {
            var arr = new int[0];

            Assert.AreEqual(0, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void Sum_SingleElement()
        {
            var arr = new[] { 42 };

            Assert.AreEqual(42, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void Sum_MultipleElements()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            Assert.AreEqual(15, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void Sum_NegativeValues()
        {
            var arr = new[] { -5, 3, -2, 4 };

            Assert.AreEqual(0, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void Sum_LengthsZeroToThirtyTwo_MatchReference()
        {
            for (var length = 0; length <= 32; length++)
            {
                var arr = new int[length];
                for (var i = 0; i < length; i++)
                    arr[i] = (i * 7) - 3;

                var expected = ReferenceSum(arr);

                Assert.AreEqual(expected, arr.AsReadOnlySpan().Sum(), $"Mismatch for length {length}");
            }
        }

        [Test]
        public void Sum_UnalignedSpans_MatchReference()
        {
            var arr = new int[100];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i * 13) - 41;

            // Offsets 0-3 give different 16-byte alignment states
            for (var offset = 0; offset < 4; offset++)
            {
                for (var length = 0; length <= 20; length++)
                {
                    if (offset + length > arr.Length)
                        continue;

                    var span = arr.AsSpan(offset, length);
                    var expected = ReferenceSum(span.ToArray());

                    Assert.AreEqual(expected, span.Sum(),
                        $"Mismatch for offset {offset}, length {length}");
                }
            }
        }

        [Test]
        public void Sum_LargeArray()
        {
            var arr = new int[10000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 97) - 40;

            Assert.AreEqual(ReferenceSum(arr), arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void Sum_SpanOverload_MatchesReadOnlyOverload()
        {
            var arr = new int[64];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i * 3) - 17;

            Assert.AreEqual(arr.AsReadOnlySpan().Sum(), arr.AsSpan().Sum());
        }

        #endregion

        #region Sum (float)

        [Test]
        public void FloatSum_EmptySpan_ReturnsZero()
        {
            var arr = new float[0];

            Assert.AreEqual(0f, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void FloatSum_SingleElement()
        {
            var arr = new[] { 42.5f };

            Assert.AreEqual(42.5f, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void FloatSum_MultipleElements()
        {
            var arr = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

            AssertFloatApproximately(ReferenceSum(arr), arr.AsReadOnlySpan().Sum(), "FloatSum_MultipleElements");
        }

        [Test]
        public void FloatSum_NegativeValues()
        {
            var arr = new[] { -5f, 3f, -2f, 4f };

            AssertFloatApproximately(0f, arr.AsReadOnlySpan().Sum(), "FloatSum_NegativeValues");
        }

        [Test]
        public void FloatSum_LengthsZeroToThirtyTwo_MatchReference()
        {
            for (var length = 0; length <= 32; length++)
            {
                var arr = new float[length];
                for (var i = 0; i < length; i++)
                    arr[i] = (i % 10) * 0.5f - 2f;

                AssertFloatApproximately(ReferenceSum(arr), arr.AsReadOnlySpan().Sum(), $"Length {length}");
            }
        }

        [Test]
        public void FloatSum_UnalignedSpans_MatchReference()
        {
            var arr = new float[100];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 7) * 0.25f - 1f;

            // Offsets 0-3 give different 16-byte alignment states
            for (var offset = 0; offset < 4; offset++)
            {
                for (var length = 0; length <= 20; length++)
                {
                    if (offset + length > arr.Length)
                        continue;

                    var span = arr.AsSpan(offset, length);
                    var expected = ReferenceSum(span.ToArray());

                    AssertFloatApproximately(expected, span.Sum(), $"Offset {offset}, length {length}");
                }
            }
        }

        [Test]
        public void FloatSum_LargeArray()
        {
            var arr = new float[10000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 97) * 0.01f - 0.5f;

            AssertFloatApproximately(ReferenceSum(arr), arr.AsReadOnlySpan().Sum(), "FloatSum_LargeArray");
        }

        [Test]
        public void FloatSum_SpanOverload_MatchesReadOnlyOverload()
        {
            var arr = new float[64];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 13) * 0.1f - 0.6f;

            Assert.AreEqual(arr.AsReadOnlySpan().Sum(), arr.AsSpan().Sum());
        }

        #endregion

        #region Sum (double)

        [Test]
        public void DoubleSum_EmptySpan_ReturnsZero()
        {
            var arr = new double[0];

            Assert.AreEqual(0.0, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void DoubleSum_SingleElement()
        {
            var arr = new[] { 42.5 };

            Assert.AreEqual(42.5, arr.AsReadOnlySpan().Sum());
        }

        [Test]
        public void DoubleSum_MultipleElements()
        {
            var arr = new[] { 0.1, 0.2, 0.3, 0.4, 0.5 };

            AssertDoubleApproximately(ReferenceSum(arr), arr.AsReadOnlySpan().Sum(), "DoubleSum_MultipleElements");
        }

        [Test]
        public void DoubleSum_NegativeValues()
        {
            var arr = new[] { -5.0, 3.0, -2.0, 4.0 };

            AssertDoubleApproximately(0.0, arr.AsReadOnlySpan().Sum(), "DoubleSum_NegativeValues");
        }

        [Test]
        public void DoubleSum_LengthsZeroToThirtyTwo_MatchReference()
        {
            for (var length = 0; length <= 32; length++)
            {
                var arr = new double[length];
                for (var i = 0; i < length; i++)
                    arr[i] = (i % 10) * 0.5 - 2.0;

                AssertDoubleApproximately(ReferenceSum(arr), arr.AsReadOnlySpan().Sum(), $"Length {length}");
            }
        }

        [Test]
        public void DoubleSum_UnalignedSpans_MatchReference()
        {
            var arr = new double[100];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 7) * 0.25 - 1.0;

            // Offsets 0-3 give different 32-byte alignment states
            for (var offset = 0; offset < 4; offset++)
            {
                for (var length = 0; length <= 20; length++)
                {
                    if (offset + length > arr.Length)
                        continue;

                    var span = arr.AsSpan(offset, length);
                    var expected = ReferenceSum(span.ToArray());

                    AssertDoubleApproximately(expected, span.Sum(), $"Offset {offset}, length {length}");
                }
            }
        }

        [Test]
        public void DoubleSum_LargeArray()
        {
            var arr = new double[10000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 97) * 0.01 - 0.5;

            AssertDoubleApproximately(ReferenceSum(arr), arr.AsReadOnlySpan().Sum(), "DoubleSum_LargeArray");
        }

        [Test]
        public void DoubleSum_SpanOverload_MatchesReadOnlyOverload()
        {
            var arr = new double[64];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 13) * 0.1 - 0.6;

            Assert.AreEqual(arr.AsReadOnlySpan().Sum(), arr.AsSpan().Sum());
        }

        #endregion

        #region NeumaierSum

        [Test]
        public void NeumaierSum_EmptySpan_ReturnsZero()
        {
            var arr = new float[0];

            Assert.AreEqual(0f, arr.AsSpan().NeumaierSum());
        }

        [Test]
        public void NeumaierSum_SingleElement()
        {
            var arr = new[] { 42.5f };

            Assert.AreEqual(42.5f, arr.AsSpan().NeumaierSum());
        }

        [Test]
        public void NeumaierSum_MultipleElements()
        {
            var arr = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

            AssertFloatApproximately(KahanReferenceSum(arr), arr.AsSpan().NeumaierSum(), "NeumaierSum_MultipleElements");
        }

        [Test]
        public void NeumaierSum_NegativeValues()
        {
            var arr = new[] { -5f, 3f, -2f, 4f };

            AssertFloatApproximately(0f, arr.AsSpan().NeumaierSum(), "NeumaierSum_NegativeValues");
        }

        [Test]
        public void NeumaierSum_LengthsZeroToThirtyTwo_MatchReference()
        {
            for (var length = 0; length <= 32; length++)
            {
                var arr = new float[length];
                for (var i = 0; i < length; i++)
                    arr[i] = (i % 10) * 0.5f - 2f;

                AssertFloatApproximately(KahanReferenceSum(arr), arr.AsSpan().NeumaierSum(), $"Length {length}");
            }
        }

        [Test]
        public void NeumaierSum_UnalignedSpans_MatchReference()
        {
            var arr = new float[100];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 7) * 0.25f - 1f;

            // Offsets 0-3 give different 16-byte alignment states
            for (var offset = 0; offset < 4; offset++)
            {
                for (var length = 0; length <= 20; length++)
                {
                    if (offset + length > arr.Length)
                        continue;

                    var span = arr.AsSpan(offset, length);
                    var expected = KahanReferenceSum(span.ToArray());

                    AssertFloatApproximately(expected, span.NeumaierSum(), $"Offset {offset}, length {length}");
                }
            }
        }

        [Test]
        public void NeumaierSum_LargeArray()
        {
            var arr = new float[10000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 97) * 0.01f - 0.5f;

            AssertFloatApproximately(KahanReferenceSum(arr), arr.AsSpan().NeumaierSum(), "NeumaierSum_LargeArray");
        }

        [Test]
        public void NeumaierSum_SpanOverload_MatchesReadOnlyOverload()
        {
            var arr = new float[64];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 13) * 0.1f - 0.6f;

            Assert.AreEqual(arr.AsSpan().NeumaierSum(), arr.AsReadOnlySpan().NeumaierSum());
        }

        #endregion

        #region SumSqr

        [Test]
        public void SumSqr_EmptySpan_ReturnsZero()
        {
            var arr = new float[0];

            Assert.AreEqual(0f, arr.AsSpan().SumSqr());
        }

        [Test]
        public void SumSqr_SingleElement()
        {
            var arr = new[] { 3f };

            Assert.AreEqual(9f, arr.AsSpan().SumSqr());
        }

        [Test]
        public void SumSqr_MultipleElements()
        {
            var arr = new[] { 1f, 2f, 3f, 4f, 5f };

            Assert.AreEqual(55f, arr.AsSpan().SumSqr());
        }

        [Test]
        public void SumSqr_NegativeValues()
        {
            var arr = new[] { -5f, 3f, -2f, 4f };

            Assert.AreEqual(54f, arr.AsSpan().SumSqr());
        }

        [Test]
        public void SumSqr_LengthsZeroToThirtyTwo_MatchReference()
        {
            for (var length = 0; length <= 32; length++)
            {
                var arr = new float[length];
                for (var i = 0; i < length; i++)
                    arr[i] = (i % 10) * 0.5f - 2f;

                AssertFloatApproximately(SumSqrReferenceSum(arr), arr.AsSpan().SumSqr(), $"Length {length}");
            }
        }

        [Test]
        public void SumSqr_UnalignedSpans_MatchReference()
        {
            var arr = new float[100];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 7) * 0.25f - 1f;

            // Offsets 0-3 give different 16-byte alignment states
            for (var offset = 0; offset < 4; offset++)
            {
                for (var length = 0; length <= 20; length++)
                {
                    if (offset + length > arr.Length)
                        continue;

                    var span = arr.AsSpan(offset, length);
                    var expected = SumSqrReferenceSum(span.ToArray());

                    AssertFloatApproximately(expected, span.SumSqr(), $"Offset {offset}, length {length}");
                }
            }
        }

        [Test]
        public void SumSqr_LargeArray()
        {
            var arr = new float[10000];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 97) * 0.01f - 0.5f;

            AssertFloatApproximately(SumSqrReferenceSum(arr), arr.AsSpan().SumSqr(), "SumSqr_LargeArray");
        }

        [Test]
        public void SumSqr_SpanOverload_MatchesReadOnlyOverload()
        {
            var arr = new float[64];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = (i % 13) * 0.1f - 0.6f;

            Assert.AreEqual(arr.AsSpan().SumSqr(), arr.AsReadOnlySpan().SumSqr());
        }

        #endregion

        #region Variance

        [Test]
        public void Variance_KnownUniformDistribution()
        {
            var values = new[] { 1f, 2f, 3f, 4f };
            var probabilities = new[] { 0.25f, 0.25f, 0.25f, 0.25f };

            // mean = 2.5, variance = 0.25 * (2.25 + 0.25 + 0.25 + 2.25) = 1.25
            AssertFloatApproximately(1.25f, values.AsReadOnlySpan().Variance(probabilities, 2.5f), "Variance_KnownUniformDistribution");
        }

        [Test]
        public void Variance_SingleValue_ReturnsZero()
        {
            var values = new[] { 5f };
            var probabilities = new[] { 1f };

            AssertFloatApproximately(0f, values.AsReadOnlySpan().Variance(probabilities, 5f), "Variance_SingleValue_ReturnsZero");
        }

        [Test]
        public void Variance_AllEqualValues_ReturnsZero()
        {
            var values = new[] { 3f, 3f, 3f, 3f };
            var probabilities = new[] { 0.25f, 0.25f, 0.25f, 0.25f };

            AssertFloatApproximately(0f, values.AsReadOnlySpan().Variance(probabilities, 3f), "Variance_AllEqualValues_ReturnsZero");
        }

        [Test]
        public void Variance_WeightedProbabilities()
        {
            var values = new[] { 10f, 0f };
            var probabilities = new[] { 0.9f, 0.1f };

            // mean = 9, variance = 0.9 * 1 + 0.1 * 81 = 9
            AssertFloatApproximately(9f, values.AsReadOnlySpan().Variance(probabilities, 9f), "Variance_WeightedProbabilities");
        }

        [Test]
        public void Variance_MismatchedLengths_ThrowsArgumentException()
        {
            var values = new[] { 1f, 2f, 3f };
            var probabilities = new[] { 0.5f, 0.5f };

            Assert.Throws<ArgumentException>(
                () => values.AsReadOnlySpan().Variance(probabilities, 1f));
        }

        [Test]
        public void Variance_LargeDistribution_MatchesReference()
        {
            var count = 10000;
            var values = new float[count];
            var probabilities = new float[count];
            for (var i = 0; i < count; i++)
            {
                values[i] = (i % 97) * 0.1f - 5f;
                probabilities[i] = 1f / count;
            }

            var mean = 0f;
            for (var i = 0; i < count; i++)
                mean += values[i] * probabilities[i];

            var expected = VarianceReference(values, probabilities, mean);

            AssertFloatApproximately(expected, values.AsReadOnlySpan().Variance(probabilities, mean), "Variance_LargeDistribution_MatchesReference");
        }

        #endregion

        #region StandardDeviation

        [Test]
        public void StandardDeviation_KnownUniformDistribution()
        {
            var values = new[] { 1f, 2f, 3f, 4f };
            var probabilities = new[] { 0.25f, 0.25f, 0.25f, 0.25f };

            AssertFloatApproximately(MathF.Sqrt(1.25f), values.AsReadOnlySpan().StandardDeviation(probabilities, 2.5f),
                "StandardDeviation_KnownUniformDistribution");
        }

        [Test]
        public void StandardDeviation_SingleValue_ReturnsZero()
        {
            var values = new[] { 5f };
            var probabilities = new[] { 1f };

            AssertFloatApproximately(0f, values.AsReadOnlySpan().StandardDeviation(probabilities, 5f),
                "StandardDeviation_SingleValue_ReturnsZero");
        }

        [Test]
        public void StandardDeviation_WeightedProbabilities()
        {
            var values = new[] { 10f, 0f };
            var probabilities = new[] { 0.9f, 0.1f };

            AssertFloatApproximately(3f, values.AsReadOnlySpan().StandardDeviation(probabilities, 9f),
                "StandardDeviation_WeightedProbabilities");
        }

        [Test]
        public void StandardDeviation_MatchesSqrtOfReferenceVariance()
        {
            var values = new[] { 1f, 4f, 9f, 16f, 25f };
            var probabilities = new[] { 0.1f, 0.2f, 0.2f, 0.2f, 0.3f };

            var mean = 0f;
            for (var i = 0; i < values.Length; i++)
                mean += values[i] * probabilities[i];

            var expected = MathF.Sqrt(VarianceReference(values, probabilities, mean));

            AssertFloatApproximately(expected, values.AsReadOnlySpan().StandardDeviation(probabilities, mean),
                "StandardDeviation_MatchesSqrtOfReferenceVariance");
        }

        [Test]
        public void StandardDeviation_MismatchedLengths_ThrowsArgumentException()
        {
            var values = new[] { 1f, 2f, 3f };
            var probabilities = new[] { 0.5f, 0.5f };

            Assert.Throws<ArgumentException>(
                () => values.AsReadOnlySpan().StandardDeviation(probabilities, 1f));
        }

        #endregion

        #region IdxMax

        [Test]
        public void IdxMax_MaxAtStart()
        {
            var arr = new[] { 9, 1, 2, 3 };

            Assert.AreEqual(0, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax_MaxInMiddle()
        {
            var arr = new[] { 1, 9, 2, 3 };

            Assert.AreEqual(1, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax_MaxAtEnd()
        {
            var arr = new[] { 1, 2, 3, 9 };

            Assert.AreEqual(3, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax_Ties_ReturnsFirstIndex()
        {
            var arr = new[] { 3, 5, 5, 1 };

            Assert.AreEqual(1, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax_SingleElement()
        {
            var arr = new[] { 7 };

            Assert.AreEqual(0, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax_AllEqual_ReturnsFirstIndex()
        {
            var arr = new[] { 4, 4, 4, 4 };

            Assert.AreEqual(0, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax_NegativeValues()
        {
            var arr = new[] { -5, -1, -3 };

            Assert.AreEqual(1, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax_StringValues()
        {
            var arr = new[] { "apple", "banana", "cherry" };

            Assert.AreEqual(2, arr.AsReadOnlySpan().IdxMax());
        }

        [Test]
        public void IdxMax()
        {
            var span = new[] { 1, 2, 7, 3, 1, 4 }.AsReadOnlySpan();
            var idx = span.IdxMax();
            Assert.AreEqual(2, idx);
        }

        [Test]
        public void IdxMaxDuplicate()
        {
            var span = new[] { 1, 2, 7, 3, 7, 4 }.AsReadOnlySpan();
            var idx = span.IdxMax();
            Assert.AreEqual(2, idx);
        }

        #endregion

        #region IdxMin

        [Test]
        public void IdxMin_MinAtStart()
        {
            var arr = new[] { 1, 2, 3, 4 };

            Assert.AreEqual(0, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin_MinInMiddle()
        {
            var arr = new[] { 3, 1, 2, 4 };

            Assert.AreEqual(1, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin_MinAtEnd()
        {
            var arr = new[] { 3, 2, 1, 4 };

            Assert.AreEqual(2, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin_Ties_ReturnsFirstIndex()
        {
            var arr = new[] { 2, 0, 0, 1 };

            Assert.AreEqual(1, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin_SingleElement()
        {
            var arr = new[] { 7 };

            Assert.AreEqual(0, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin_AllEqual_ReturnsFirstIndex()
        {
            var arr = new[] { 4, 4, 4, 4 };

            Assert.AreEqual(0, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin_NegativeValues()
        {
            var arr = new[] { -1, -5, -3 };

            Assert.AreEqual(1, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin_StringValues()
        {
            var arr = new[] { "cherry", "banana", "apple" };

            Assert.AreEqual(2, arr.AsReadOnlySpan().IdxMin());
        }

        [Test]
        public void IdxMin()
        {
            var span = new[] { 1, 2, 7, 3, -2, 2, 4 }.AsReadOnlySpan();
            var idx = span.IdxMin();
            Assert.AreEqual(4, idx);
        }

        [Test]
        public void IdxMinDuplicate()
        {
            var span = new[] { 1, 2, 7, 1, 7, 4 }.AsReadOnlySpan();
            var idx = span.IdxMin();
            Assert.AreEqual(0, idx);
        }

        [Test]
        public void IdxMinDouble()
        {
            var span = new double[] { 1, 2, 7, 3, -2, 2, 4 }.AsReadOnlySpan();
            var idx = span.IdxMin();
            Assert.AreEqual(4, idx);
        }

        #endregion
    }
}
