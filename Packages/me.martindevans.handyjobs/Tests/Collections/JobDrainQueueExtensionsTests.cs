using me.martindevans.handyjobs.Primitives.Collections;
using NUnit.Framework;
using Unity.Collections;

namespace Tests.Collections
{
    public class JobDrainQueueExtensionsTests
    {
        [Test]
        public void DrainTo_AppendsQueuedItemsInOrder_AndHandlesEmptyQueue()
        {
            var queue = new NativeQueue<int>(Allocator.TempJob);
            var list = new NativeList<int>(Allocator.TempJob) { 100, 200 };
            try
            {
                queue.Enqueue(1);
                queue.Enqueue(2);
                queue.Enqueue(3);

                queue.DrainTo(list, default).Complete();

                Assert.AreEqual(5, list.Length);
                Assert.AreEqual(100, list[0]);
                Assert.AreEqual(200, list[1]);
                Assert.AreEqual(1, list[2]);
                Assert.AreEqual(2, list[3]);
                Assert.AreEqual(3, list[4]);
                Assert.AreEqual(0, queue.Count);

                queue.DrainTo(list, default).Complete();

                Assert.AreEqual(5, list.Length);
                Assert.AreEqual(0, queue.Count);
            }
            finally
            {
                list.Dispose();
                queue.Dispose();
            }
        }
    }
}
