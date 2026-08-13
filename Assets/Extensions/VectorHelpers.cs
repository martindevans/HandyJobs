using System;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Extensions
{
    public static class VectorHelpers
    {
        private static float CopySign(float a, float b)
        {
            if (b >= 0)
                return math.abs(a);
            else
                return -math.abs(a);
        }

        public static float3 Perpendicular(this Vector3 input)
        {
            return ((float3)input).Perpendicular();
        }

        public static float3 Perpendicular(this float3 input)
        {
            return new float3(
                +CopySign(input.z, input.x),
                +CopySign(input.z, input.y),
                -CopySign(math.abs(input.x) + math.abs(input.y), input.z)
            );
        }

        public static float NextGaussian(this System.Random random, float mean, float stdDev)
        {
            var u1 = 1 - (float)random.NextDouble();
            var u2 = 1 - (float)random.NextDouble();

            var randStdNormal = MathF.Sqrt(-2 * MathF.Log(u1)) * MathF.Sin(2 * MathF.PI * u2);
            var randNormal = mean + stdDev * randStdNormal;

            return randNormal;
        }

        /// <summary>
        /// Adds a random angular spread to a velocity vector.
        /// </summary>
        /// <param name="vel">The original velocity vector.</param>
        /// <param name="rng"></param>
        /// <param name="stdDevDegrees"></param>
        /// <returns>A new velocity vector with random spread applied.</returns>
        public static float3 SpreadVelocity(this Vector3 vel, System.Random rng, float stdDevDegrees)
        {
            return ((float3)vel).SpreadVelocity(rng, stdDevDegrees);
        }

        public static float3 SpreadVelocity(this float3 vel, System.Random rng, float stdDevDegrees)
        {
            var speed = math.length(vel);
            if (speed < 0.0001f)
                return vel;

            // Angle of deflection (from center)
            var theta = rng.NextGaussian(0, stdDevDegrees);

            // Create a local coordinate system (Basis) for the velocity
            var forward = (float3)vel / speed;

            // Get a random perpendicular vector
            var perp = forward.Perpendicular();

            // Rotate it around forward
            perp = Quaternion.AngleAxis((float)rng.NextDouble() * 360, forward) * perp;

            // Rotate forward vector
            var quat = Quaternion.AngleAxis(theta, perp);
            var dir = quat * forward;

            // Preserve speed in new direction
            return dir * speed;
        }
    }
}
