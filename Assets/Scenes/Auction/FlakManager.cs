using Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class FlakManager
        : MonoBehaviour
    {
        public Bullet BulletPrefab;

        public float StdDevSpreadDegrees = 0.5f;
        private readonly System.Random _rng = new();

        public void Spawn(Vector3 pos, float speed, Projectile target)
        {
            var intercept = InterceptHelpers.TryGetIntercept(
                new InterceptHelpers.LinearVelocityObject(target.Position, target.Velocity),
                new InterceptHelpers.LinearVelocityObject(pos, Vector3.zero),
                new InterceptHelpers.ConstantSpeedProjectile(speed)
            );

            if (!intercept.HasValue)
                return;

            var ipos = target.Position + target.Velocity * intercept.Value;
            var dir = math.normalize(ipos - pos);
            var vel = dir * speed;

            vel = vel.SpreadVelocity(_rng, StdDevSpreadDegrees);

            var b = Instantiate(BulletPrefab, pos, Quaternion.identity);
            b.Init(vel, target);
        }
    }
}
