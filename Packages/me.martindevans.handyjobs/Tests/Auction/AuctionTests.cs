using Auction;
using NUnit.Framework;
using System;
using Unity.Collections;

namespace Tests.Auction
{
    public class AuctionTests
    {
        #region Helper Methods
        private static void AssertValidAssignment(int[] assignment, int agentCount, int jobCount)
        {
            for (var i = 0; i < agentCount; i++)
            {
                if (assignment[i] != -1)
                    Assert.That(assignment[i], Is.GreaterThanOrEqualTo(0).And.LessThan(jobCount),
                        $"Agent {i} assigned to invalid job {assignment[i]}");
            }

            for (var i = 0; i < agentCount; i++)
            {
                for (var j = i + 1; j < agentCount; j++)
                {
                    if (assignment[i] != -1 && assignment[i] == assignment[j])
                        Assert.Fail($"Agents {i} and {j} both assigned to job {assignment[i]}");
                }
            }
        }

        private static int CountUnassigned(int[] assignment)
        {
            var count = 0;
            for (var i = 0; i < assignment.Length; i++)
                if (assignment[i] == -1) count++;
            return count;
        }

        private static float ComputeTotalValue(float[] values, int[] assignment, int jobCount)
        {
            float total = 0;
            for (var i = 0; i < assignment.Length; i++)
            {
                if (assignment[i] != -1)
                    total += values[i * jobCount + assignment[i]];
            }
            return total;
        }
        #endregion

        [Test]
        public void BasicAssignment()
        {
            var values = new NativeArray<float>(new float[]
            {
                1, 2, 3,
                2, 3, 1,
                3, 1, 2,
            }, Allocator.Persistent);

            var assignment = new NativeArray<int>(3, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 3, 3, assignment).Complete();

            Assert.AreEqual(2, assignment[0]);
            Assert.AreEqual(1, assignment[1]);
            Assert.AreEqual(0, assignment[2]);

            assignment.Dispose();
            values.Dispose();
        }

        #region Input Validation

        [Test]
        public void Throws_InvalidValuesLength()
        {
            var values = new NativeArray<float>(6, Allocator.Persistent);
            var assignment = new NativeArray<int>(3, Allocator.Persistent);

            Assert.That(() => AuctionAlgorithm.Solve(values, 3, 3, assignment).Complete(),
                Throws.InstanceOf<ArgumentException>());

            assignment.Dispose();
            values.Dispose();
        }

        [Test]
        public void Throws_InvalidAssignmentLength()
        {
            var values = new NativeArray<float>(9, Allocator.Persistent);
            var assignment = new NativeArray<int>(2, Allocator.Persistent);

            Assert.That(() => AuctionAlgorithm.Solve(values, 3, 3, assignment).Complete(),
                Throws.InstanceOf<ArgumentException>());

            assignment.Dispose();
            values.Dispose();
        }

        #endregion

        #region Square Matrices

        [Test]
        public void SingleAgentSingleJob()
        {
            var values = new NativeArray<float>(new[] { 5f }, Allocator.Persistent);
            var assignment = new NativeArray<int>(1, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 1, 1, assignment).Complete();

            Assert.AreEqual(0, assignment[0]);

            assignment.Dispose();
            values.Dispose();
        }

        [Test]
        public void TwoAgentsTwoJobs()
        {
            var rawValues = new float[]
            {
                10, 20,
                30, 40,
            };
            var values = new NativeArray<float>(rawValues, Allocator.Persistent);
            var assignment = new NativeArray<int>(2, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 2, 2, assignment).Complete();

            var result = new int[2];
            for (var i = 0; i < 2; i++) result[i] = assignment[i];

            AssertValidAssignment(result, 2, 2);

            var total = ComputeTotalValue(rawValues, result, 2);
            Assert.GreaterOrEqual(total, 50f, "Optimal assignment yields 50 (10+40 or 20+30)");

            assignment.Dispose();
            values.Dispose();
        }

        [Test]
        public void AllEqualValues()
        {
            var rawValues = new float[]
            {
                7, 7, 7,
                7, 7, 7,
                7, 7, 7,
            };
            var values = new NativeArray<float>(rawValues, Allocator.Persistent);
            var assignment = new NativeArray<int>(3, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 3, 3, assignment).Complete();

            var result = new int[3];
            for (var i = 0; i < 3; i++) result[i] = assignment[i];

            AssertValidAssignment(result, 3, 3);
            Assert.AreEqual(0, CountUnassigned(result), "All agents should be assigned in square matrix");

            assignment.Dispose();
            values.Dispose();
        }

        [Test]
        public void DiagonalDominant()
        {
            var rawValues = new float[]
            {
                10, 1, 1,
                1, 10, 1,
                1, 1, 10,
            };
            var values = new NativeArray<float>(rawValues, Allocator.Persistent);
            var assignment = new NativeArray<int>(3, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 3, 3, assignment).Complete();

            var result = new int[3];
            for (var i = 0; i < 3; i++) result[i] = assignment[i];

            AssertValidAssignment(result, 3, 3);
            Assert.AreEqual(0, CountUnassigned(result));

            var total = ComputeTotalValue(rawValues, result, 3);
            Assert.AreEqual(30f, total, "Should get 10+10+10 on diagonal");

            assignment.Dispose();
            values.Dispose();
        }

        #endregion

        #region Rectangular: More Agents Than Jobs

        [Test]
        public void ThreeAgentsTwoJobs()
        {
            var rawValues = new float[]
            {
                10, 5,
                5, 10,
                100, 100,
            };
            var values = new NativeArray<float>(rawValues, Allocator.Persistent);
            var assignment = new NativeArray<int>(3, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 3, 2, assignment).Complete();

            var result = new int[3];
            for (var i = 0; i < 3; i++) result[i] = assignment[i];

            AssertValidAssignment(result, 3, 2);
            Assert.AreEqual(1, CountUnassigned(result), "Exactly one agent should be unassigned");

            assignment.Dispose();
            values.Dispose();
        }

        [Test]
        public void NegativeValues_AllowUnassigned()
        {
            var rawValues = new float[]
            {
                10, 10,
                5, 5,
                -10, -10,
            };
            var values = new NativeArray<float>(rawValues, Allocator.Persistent);
            var assignment = new NativeArray<int>(3, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 3, 2, assignment).Complete();

            var result = new int[3];
            for (var i = 0; i < 3; i++) result[i] = assignment[i];

            AssertValidAssignment(result, 3, 2);
            Assert.GreaterOrEqual(CountUnassigned(result), 1, "At least one agent should remain unassigned");

            assignment.Dispose();
            values.Dispose();
        }

        #endregion

        #region Rectangular: More Jobs Than Agents

        [Test]
        public void TwoAgentsThreeJobs()
        {
            var rawValues = new float[]
            {
                10, 5, 1,
                1, 10, 5,
            };
            var values = new NativeArray<float>(rawValues, Allocator.Persistent);
            var assignment = new NativeArray<int>(2, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 2, 3, assignment).Complete();

            var result = new int[2];
            for (var i = 0; i < 2; i++) result[i] = assignment[i];

            AssertValidAssignment(result, 2, 3);
            Assert.AreEqual(0, CountUnassigned(result), "All agents should be assigned");

            assignment.Dispose();
            values.Dispose();
        }

        #endregion

        #region Edge Cases

        [Test]
        public void EmptyArrays()
        {
            var values = new NativeArray<float>(0, Allocator.Persistent);
            var assignment = new NativeArray<int>(0, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 0, 0, assignment).Complete();

            Assert.IsEmpty(assignment);

            assignment.Dispose();
            values.Dispose();
        }

        [Test]
        public void NegativeValues()
        {
            var rawValues = new float[]
            {
                -1, -5, -3,
                -4, -2, -6,
                -3, -6, -1,
            };
            var values = new NativeArray<float>(rawValues, Allocator.Persistent);
            var assignment = new NativeArray<int>(3, Allocator.Persistent);

            AuctionAlgorithm.Solve(values, 3, 3, assignment).Complete();

            var result = new int[3];
            for (var i = 0; i < 3; i++)
                result[i] = assignment[i];

            AssertValidAssignment(result, 3, 3);

            assignment.Dispose();
            values.Dispose();
        }

        [Test]
        public void LargeEpsilonVsSmallEpsilon()
        {
            var rawValues = new float[]
            {
                10, 20, 30,
                30, 10, 20,
                20, 30, 10,
            };

            var values = new NativeArray<float>(rawValues, Allocator.Persistent);

            var assignmentSmall = new NativeArray<int>(3, Allocator.Persistent);
            AuctionAlgorithm.Solve(values, 3, 3, assignmentSmall, epsilon: 0.0001f).Complete();

            var assignmentLarge = new NativeArray<int>(3, Allocator.Persistent);
            AuctionAlgorithm.Solve(values, 3, 3, assignmentLarge, epsilon: 1f).Complete();

            for (var i = 0; i < 3; i++)
                Assert.AreEqual(assignmentSmall[i], assignmentLarge[i],
                    $"Agent {i} assignment differs between small and large epsilon");

            assignmentSmall.Dispose();
            assignmentLarge.Dispose();
            values.Dispose();
        }

        #endregion
    }
}
