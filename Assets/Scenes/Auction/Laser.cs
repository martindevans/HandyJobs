using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Auction
{
    public class Laser
        : BaseWeaponSystem
    {
        public LineRenderer Line;

        public float Damage = 1;

        protected override void Update()
        {
            // Clear line
            Line.positionCount = 2;
            Line.SetPosition(0, Pivot.position);
            Line.SetPosition(1, Pivot.position);

            base.Update();
        }

        protected override void UpdateFacingTarget()
        {
            base.UpdateFacingTarget();

            // Do damage
            var tgtpos = Target.transform.position;
            var dist = math.distance(tgtpos, Pivot.position);
            var dropoff = math.pow(0.5f, dist / 100f);
            Target.LaserDamage(Damage * dropoff * Time.deltaTime);

            // Draw line to target
            Line.SetPosition(1, tgtpos);
        }
    }
}
