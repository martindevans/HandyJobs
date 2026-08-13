using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class Laser
        : BaseWeaponSystem
    {
        private Projectile _target;

        public Transform PivotTransform;
        public override Transform Pivot => PivotTransform;
        public LineRenderer Line;

        public float Damage = 1;
        public float RotationSpeed = 90;

        public override void Assign([CanBeNull] Projectile target)
        {
            _target = target;
        }

        private void Update()
        {
            // Clear line
            Line.positionCount = 2;
            Line.SetPosition(0, Pivot.position);
            Line.SetPosition(1, Pivot.position);

            if (_target)
            {
                // Turn to target
                var toTgt = _target.transform.position - Pivot.position;
                var rot = Quaternion.LookRotation(toTgt, Vector3.up);
                Pivot.rotation = Quaternion.RotateTowards(Pivot.rotation, rot, RotationSpeed * Time.deltaTime);

                // Check if we're facing target
                if (Vector3.Angle(Pivot.forward, toTgt) < 2)
                {
                    // Do damage
                    var tgtpos = _target.transform.position;
                    var dist = math.distance(tgtpos, Pivot.position);
                    var dropoff = math.pow(0.5f, dist / 100f);
                    _target.LaserDamage(Damage * dropoff * Time.deltaTime);

                    // Draw line to target
                    Line.SetPosition(1, tgtpos);
                }
            }
        }
    }
}
