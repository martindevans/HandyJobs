using System.Collections;
using System.Collections.Generic;
using Auction;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace Scenes.Auction
{
    public class ProjectileSpawner
        : MonoBehaviour
    {
        public float Period = 1;
        public float Radius = 500;
        public float MinSpeed = 5;
        public float MaxSpeed = 50;

        public float BurstInterval = 10f;
        public int BurstCount = 10;
        public float BurstSpread = 15f;

        public float PerBulletPenalty = 0.1f;

        private readonly List<Projectile> _projectiles = new();

        public ProjectileStatistics Statistics;
        public Projectile Prefab;
        public BaseWeaponSystem[] Guns;

        private void OnEnable()
        {
            StartCoroutine(SpawnLoop());
            StartCoroutine(BurstSpawnLoop());
            StartCoroutine(AssignmentLoop());
        }

        private IEnumerator SpawnLoop()
        {
            var rng = new Random();
            while (true)
            {
                yield return null;
                yield return new WaitForSeconds(Period);

                var angle = (float)rng.NextDouble() * math.PI2;
                math.sincos(angle, out var s, out var c);

                var pos = new Vector3(s * Radius, 25, c * Radius);
                SpawnProjectile(pos, rng, MinSpeed, MaxSpeed);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private IEnumerator BurstSpawnLoop()
        {
            var rng = new Random();
            while (true)
            {
                yield return null;
                yield return new WaitForSeconds((float)rng.NextDouble() * BurstInterval);

                var angle = (float)rng.NextDouble() * math.PI2;
                math.sincos(angle, out var s, out var c);

                var centerPos = new Vector3(s * Radius, 25, c * Radius);

                var speedRange = MaxSpeed - MinSpeed;
                var burstRange = speedRange * ((float)rng.NextDouble() * 0.5f + 0.25f);
                var burstMin = MinSpeed + (float)rng.NextDouble() * (speedRange - burstRange);
                var burstMax = burstMin + burstRange;

                for (var i = 0; i < BurstCount; i++)
                {
                    var offset = new Vector3(
                        ((float)rng.NextDouble() - 0.5f) * 2f * BurstSpread,
                        ((float)rng.NextDouble() - 0.5f) * 2f * BurstSpread,
                        ((float)rng.NextDouble() - 0.5f) * 2f * BurstSpread
                    );
                    var pos = centerPos + offset;
                    SpawnProjectile(pos, rng, burstMin, burstMax);
                }
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void SpawnProjectile(Vector3 pos, Random rng, float minSpeed, float maxSpeed)
        {
            var dir = (-pos).normalized;
            var vel = dir * ((float)rng.NextDouble() * (maxSpeed - minSpeed) + minSpeed);

            var proj = Instantiate(Prefab, pos, Quaternion.identity);
            proj.Velocity = vel;
            proj.Statistics = Statistics;

            _projectiles.Add(proj);
        }

        private IEnumerator AssignmentLoop()
        {
            while (true)
            {
                yield return null;
                yield return new WaitForSeconds(0.1f);

                // Clean up list
                _projectiles.RemoveAll(a => !a);

                // Setup values for each job
                var values = new NativeArray<float>(Guns.Length * _projectiles.Count, Allocator.Persistent);
                for (var i = 0; i < Guns.Length; i++)
                {
                    var gun = Guns[i];
                    var gunPos = gun.Pivot.position;
                    var gunFwd = gun.Pivot.forward;
                    var gunPivotDir = gunPos.normalized;

                    for (var j = 0; j < _projectiles.Count; j++)
                    {
                        var proj = _projectiles[j];

                        var vel = proj.RigidBody.linearVelocity;
                        var speed = math.length(vel);

                        var toProj = proj.transform.position - gunPos;
                        var dist = math.length(toProj);
                        var toProjDir = toProj / dist;

                        var closingSpeed = math.max(1f, -math.dot(vel.normalized, toProjDir) * speed);
                        var arrivalTime = math.max(1f, dist / closingSpeed);

                        // base value
                        var value = 100f;

                        // Prefer targets that the gun is currently facing
                        value += math.clamp(math.dot(toProjDir, gunFwd), 0.1f, 1);

                        // Prefer targets that arrive soon
                        value /= arrivalTime;

                        // Slightly prefer targets that are in the "natural" section of sky for this gun
                        value += math.dot(toProjDir, gunPivotDir);

                        // Subtract a tiny amount for each bullet already fired at this threat
                        value -= proj.AttachedBulletsCount * PerBulletPenalty;

                        values[i * _projectiles.Count + j] = value;
                    }
                }

                // Start assignment
                using var assignment = new NativeArray<int>(Guns.Length, Allocator.Persistent);
                var handle = AuctionAlgorithm.Solve(values, Guns.Length, _projectiles.Count, assignment);
                handle = values.Dispose(handle);

                // Wait for job
                while (!handle.IsCompleted)
                    yield return null;
                handle.Complete();

                // Hand out assignments
                for (var i = 0; i < assignment.Length; i++)
                {
                    var idx = assignment[i];
                    if (idx < 0)
                        Guns[i].Assign(null);
                    else
                        Guns[i].Assign(_projectiles[idx]);
                }
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}
