using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class FlakShell
        : BaseDefenceProjectile
    {
        /// <summary>
        /// Self destruct time
        /// </summary>
        public float FuzeDuration = 10;

        /// <summary>
        /// Prefab to spawn on death
        /// </summary>
        public GameObject ExplosionPrefab;

        protected override void Update()
        {
            base.Update();

            FuzeDuration -= Time.deltaTime;

            if (FuzeDuration <= 0)
                Detonate();
        }

        private void Detonate()
        {
            if (ExplosionPrefab)
                Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        public void Init(float3 vel, Projectile target, float lifetime)
        {
            RigidBody.linearVelocity = vel;
            FuzeDuration = lifetime;

            target.AddBullet(this);
        }
    }
}
