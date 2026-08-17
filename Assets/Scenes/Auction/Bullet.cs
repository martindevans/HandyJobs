using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class Bullet
        : BaseDefenceProjectile
    {
        public Projectile Target;
        public ProjectileStatistics Statistics;

        public void Init(float3 vel, Projectile target)
        {
            RigidBody.linearVelocity = vel;
            Target = target;

            if (target)
                target.AddBullet(this);
        }

        protected override void Update()
        {
            base.Update();

            if (DistanceFromOrigin > 1000)
            {
                if (Target)
                    Target.Statistics.ReportBulletMiss();
                DestroySelf();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            DestroySelf();
        }

        private void DestroySelf()
        {
            if (Target)
                Target.RemoveBullet(this);
            Destroy(gameObject);
        }
    }
}
