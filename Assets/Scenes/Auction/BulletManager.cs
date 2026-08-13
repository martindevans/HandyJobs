using System;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class BulletManager
        : MonoBehaviour
    {
        public Bullet BulletPrefab;

        public float StdDevSpreadDegrees = 0.5f;
        private readonly System.Random _rng = new();

        public void Spawn(Vector3 pos, float speed, Projectile target)
        {
            var intercept = TryGetIntercept(
                new LinearVelocityObject(target.Position, target.Velocity),
                new LinearVelocityObject(pos, Vector3.zero),
                new ConstantSpeedProjectile(speed)
            );

            if (!intercept.HasValue)
                return;

            var ipos = target.Position + target.Velocity * intercept.Value;
            var dir = math.normalize(ipos - pos);
            var vel = dir * speed;

            vel = SpreadVelocity(vel);

            var b = Instantiate(BulletPrefab, pos, Quaternion.identity);
            b.Init(vel, target);
        }

        /// <summary>
        /// Try to get the intercept for a target, shooter and bullet with constant velocity
        /// </summary>
        /// <param name="target">Target to shoot at</param>
        /// <param name="shooter">The shooter</param>
        /// <param name="projectile">The projectile from the shooter</param>
        /// <returns>How far into the future the intercept occurs. Use targetPos + targetVelocity to get intercept point</returns>
        public static float? TryGetIntercept(in LinearVelocityObject target, in LinearVelocityObject shooter, in ConstantSpeedProjectile projectile)
        {
            // Subtract off shooter velocity to transform it to local velocity
            var tgtLocalVel = target.Velocity - shooter.Velocity;

            // Subtract of shooter position to move them to the origin
            var tgtLocalPos = target.Position - shooter.Position;

            var a = math.lengthsq(tgtLocalVel) - math.pow(projectile.Speed, 2);
            var b = 2f * math.dot(tgtLocalPos, tgtLocalVel);
            var c = math.lengthsq(tgtLocalPos);

            // Solve quadratic: a * t^2 + b * t + c = 0
            var discriminant = b * b - 4f * a * c;

            // No solution (projectile too slow)
            if (discriminant < 0f)
                return default;

            var sqrtDisc = math.sqrt(discriminant);
            var denom = math.rcp(2 * a);

            // Calculate times to intercept
            var t1 = (-b + sqrtDisc) * denom;
            var t2 = (-b - sqrtDisc) * denom;

            // Take the smallest intercept
            var t = math.min(t1, t2);

            // If that was negative, take the other one
            if (t < 0f)
                t = math.max(t1, t2);

            // If they're both negative there's no valid intercept!
            // Could have hit in the past, but too late now.
            if (t < 0f)
                return default;

            return t;
        }

        /// <summary>
        /// An object with constant velocity
        /// </summary>
        public readonly struct LinearVelocityObject
        {
            public readonly float3 Position;
            public readonly float3 Velocity;

            public LinearVelocityObject(float3 position, float3 velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public float3 Predict(float time)
            {
                return Position + Velocity * time;
            }
        }

        /// <summary>
        /// A projectile with constant speed
        /// </summary>
        public readonly struct ConstantSpeedProjectile
        {
            /// <summary>
            /// Speed of this projectile
            /// </summary>
            public readonly float Speed;

            public ConstantSpeedProjectile(float speed)
            {
                Speed = speed;
            }
        }

        /// <summary>
        /// Adds a random angular spread to a velocity vector.
        /// </summary>
        /// <param name="vel">The original velocity vector.</param>
        /// <param name="stdDevDegrees">The standard deviation of the spread in degrees.</param>
        /// <returns>A new velocity vector with random spread applied.</returns>
        private float3 SpreadVelocity(Vector3 vel)
        {
            var speed = vel.magnitude;
            if (speed < 0.0001f)
                return vel;

            // Angle of deflection (from center)
            var theta = NextGaussian(_rng, 0, StdDevSpreadDegrees);

            // Create a local coordinate system (Basis) for the velocity
            var forward = (float3)vel / speed;

            // Get a random perpendicular vector
            var perp = Perpendicular(forward);

            // Rotate it around forward
            perp = Quaternion.AngleAxis((float)_rng.NextDouble() * 360, forward) * perp;

            // Rotate forward vector
            var quat = Quaternion.AngleAxis(theta, perp);
            var dir = quat * forward;

            // Preserve speed in new direction
            return dir * speed;
        }

        private static float CopySign(float a, float b)
        {
            if (b >= 0)
                return math.abs(a);
            else
                return -math.abs(a);
        }

        private static float3 Perpendicular(float3 input)
        {
            return new float3(
                +CopySign(input.z, input.x),
                +CopySign(input.z, input.y),
                -CopySign(math.abs(input.x) + math.abs(input.y), input.z)
            );
        }

        public static float NextGaussian(System.Random random, float mean, float stdDev)
        {
            var u1 = 1 - (float)random.NextDouble();
            var u2 = 1 - (float)random.NextDouble();

            var randStdNormal = MathF.Sqrt(-2 * MathF.Log(u1)) * MathF.Sin(2 * MathF.PI * u2);
            var randNormal = mean + stdDev * randStdNormal;

            return randNormal;
        }
    }
}
