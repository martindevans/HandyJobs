using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Auction
{
    /// <summary>
    /// Assigns workers to jobs, attempting to globally optimised the assignment
    /// such that every worker is assigned to their best job.
    /// </summary>
    public static class AuctionAlgorithm
    {
        /// <summary>
        /// Solves an assignment problem with an auction.
        ///
        /// values[agent * count + job] = value of assigning agent to job.
        ///
        /// Returns:
        /// assignment[agent] = job
        /// </summary>
        public static JobHandle Solve(
            NativeArray<float> values,
            int agentCount,
            int jobCount,
            NativeArray<int> assignment,
            float epsilon = 0.0001f,
            uint? seed = null,
            JobHandle dependsOn = default)
        {
            // Check arguments
            if (values.Length != agentCount * jobCount)
                throw new ArgumentException("Values array is the wrong size", nameof(values));
            if (assignment.Length != agentCount)
                throw new ArgumentException("Assignment array is the wrong size", nameof(assignment));

            // Initialise seed
            if (!seed.HasValue)
            {
                var s = 11;

                var initialValues = values.AsSpan()[..math.min(16, values.Length)];
                for (var i = 0; i < initialValues.Length; i++)
                    s += 17 * unchecked((int)initialValues[i]);

                seed = unchecked((uint)HashCode.Combine(s, epsilon, agentCount, jobCount, dependsOn));
            }

            // Start job
            return new AuctionJob(values, agentCount, jobCount, assignment, epsilon, seed.Value).Schedule(dependsOn);
        }

        private struct AuctionJob
            : IJob
        {
            [ReadOnly] private readonly NativeArray<float> _values;
            private readonly int _agentCount;
            private readonly int _jobCount;
            private readonly bool _allowUnassignedAgents;
            private readonly float _epsilon;
            private readonly uint _seed;

            private NativeArray<int> _assignment;

            public AuctionJob(NativeArray<float> values, int agentCount, int jobCount, NativeArray<int> assignment, float epsilon, uint seed)
            {
                _values = values;
                _agentCount = agentCount;
                _jobCount = jobCount;
                _assignment = assignment;
                _epsilon = epsilon;
                _seed = seed;

                _allowUnassignedAgents = agentCount > jobCount;
            }

            public void Execute()
            {
                // Array of job indices. Each slice is sorted in descending order of value to that agent
                var sortedOrder = new NativeArray<int>(_agentCount * _jobCount, Allocator.Temp);
                BuildSortedJobPreference(sortedOrder);

                // Current prices for each job (deducted off value)
                var prices = new NativeArray<float>(_jobCount, Allocator.Temp);
                prices.AsSpan().Fill(0);

                // Current agent assigned to each job
                var owners = new NativeArray<int>(_jobCount, Allocator.Temp);
                owners.AsSpan().Fill(-1);

                // List of unassigned agents
                var unassigned = new NativeList<int>(_agentCount, Allocator.Temp);
                for (var i = 0; i < _agentCount; i++)
                    unassigned.Add(i);

                var rng = new Unity.Mathematics.Random(_seed);
                while (unassigned.Length > 0)
                {
                    // Pick an unassigned agent
                    var agent = PickUnassigned(unassigned, ref rng);

                    // Pick the best and second best job for this worker
                    var (bestIdx, bestValue, secondValue) = PickBestJob(agent, prices, sortedOrder);

                    // If the best job is not worth bidding for just give up. This agent will remain unassigned
                    if (bestValue < 0 && _allowUnassignedAgents)
                        continue;

                    // Clamp second price, since there's an implicit "do nothing" job
                    if (_allowUnassignedAgents)
                        secondValue = math.max(0, secondValue);

                    // Raise price of best option
                    var price = prices[bestIdx] + (bestValue - secondValue) + _epsilon;

                    // Kick off whoever owned it before
                    var prev = owners[bestIdx];
                    if (prev >= 0)
                        unassigned.Add(prev);

                    // Take ownership
                    owners[bestIdx] = agent;
                    prices[bestIdx] = price;
                }

                // Copy assignments to output
                _assignment.AsSpan().Fill(-1);
                for (var i = 0; i < owners.Length; i++)
                {
                    var owner = owners[i];
                    if (owner >= 0)
                        _assignment[owner] = i;
                }
            }

            private void BuildSortedJobPreference(NativeArray<int> sortedOrder)
            {
                for (var agent = 0; agent < _agentCount; agent++)
                {
                    var offset = agent * _jobCount;

                    // Fill in the job indices
                    for (var job = 0; job < _jobCount; job++)
                        sortedOrder[offset + job] = job;

                    // Sort by value of job descending
                    var comparer = new CompareJobValueDescending(_values.Slice(offset, length: _jobCount));
                    sortedOrder.Slice(offset, _jobCount).Sort(comparer);
                }
            }

            private static int PickUnassigned(NativeList<int> unassigned, ref Unity.Mathematics.Random rng)
            {
                var idx = rng.NextInt(0, unassigned.Length);
                var agent = unassigned[idx];
                unassigned.RemoveAtSwapBack(idx);
                return agent;
            }

            private (int best, float bestValue, float secondValue) PickBestJob(int agent, NativeArray<float> prices, NativeArray<int> sortedOrder)
            {
                var slice = sortedOrder.Slice(agent * _jobCount, _jobCount);

                var bestIdx = -1;
                var bestValue = float.NegativeInfinity;
                var secondValue = float.NegativeInfinity;

                var offset = agent * _jobCount;

                for (var i = 0; i < _jobCount; i++)
                {
                    var job = slice[i];
                    var value = _values[offset + job];

                    // Early exit: Jobs are sorted in descending order of value, and prices are always positive.
                    if (value <= secondValue)
                        break;

                    var net = value - prices[job];
                    if (net > bestValue)
                    {
                        secondValue = bestValue;

                        bestValue = net;
                        bestIdx = job;
                    }
                    else if (net > secondValue)
                    {
                        secondValue = net;
                    }
                }

                return (bestIdx, bestValue, secondValue);
            }

            private readonly struct CompareJobValueDescending
                : IComparer<int>
            {
                [ReadOnly] private readonly NativeSlice<float> _valueSlice;

                public CompareJobValueDescending(NativeSlice<float> valueSlice)
                {
                    _valueSlice = valueSlice;
                }

                public int Compare(int x, int y)
                {
                    return -(_valueSlice[x].CompareTo(_valueSlice[y]));
                }
            }
        }
    }
}
