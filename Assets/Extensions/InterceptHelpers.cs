using Unity.Mathematics;

namespace Extensions
{
    public static class InterceptHelpers
    {
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
    }
}
