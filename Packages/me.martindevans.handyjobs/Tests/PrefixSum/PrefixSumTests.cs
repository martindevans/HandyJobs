using NUnit.Framework;
using Primitives;
using Unity.Collections;

namespace Tests.PrefixSum
{
    public class PrefixSumTests
    {
        [Test]
        public void BasicPrefixSum()
        {
            var values = new NativeArray<int>(new int[]
            {
                1, 2, 3,
                2, 3, 1,
                3, 1, 2,
            }, Allocator.Persistent);

            var expected = new[]
            {
                1, 3, 6, 8, 11, 12, 15, 16, 18
            };

            var output = new NativeArray<int>(9, Allocator.Persistent);

            values.PrefixSum(output, default).Complete();

            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], output[i]);

            values.Dispose();
            output.Dispose();
        }
    }
}
