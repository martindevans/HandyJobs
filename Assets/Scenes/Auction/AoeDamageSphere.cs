using System;
using System.Buffers;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class AoeDamageSphere
        : MonoBehaviour
    {
        public float Radius = 10;
        public float Damage = 5;

        private void OnEnable()
        {
            ApplyAoeDamage();
        }

        private void ApplyAoeDamage()
        {
            var pos = transform.position;

            var results = ArrayPool<Collider>.Shared.Rent(128);
            try
            {
                var count = Physics.OverlapSphereNonAlloc(transform.position, Radius, results);

                for (var i = 0; i < count; i++)
                {
                    var item = results[i];
                    if (!item.TryGetComponent<Projectile>(out var proj))
                        continue;

                    var dist = math.distance(proj.Position, pos);

                    var damage = math.unlerp(0, Radius, dist) * Damage;
                    proj.FlakDamage(damage);
                }
            }
            finally
            {
                ArrayPool<Collider>.Shared.Return(results);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}
