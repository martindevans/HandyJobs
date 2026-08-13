using UnityEngine;

namespace Scenes.Auction
{
    public class FlakGun
        : BaseWeaponSystem
    {
        public FlakManager FlakManager;
        public ProjectileStatistics Statistics;

        public float BulletsPerSecond = 1;
        public float BulletSpeed = 10;

        private float _timeAccumulator;

        private void OnEnable()
        {
            if (!FlakManager)
                FlakManager = GetComponentInParent<FlakManager>();
            if (!Statistics)
                Statistics = GetComponentInParent<ProjectileStatistics>();
        }

        protected override void UpdateFacingTarget()
        {
            base.UpdateFacingTarget();

            _timeAccumulator += Time.deltaTime;

            var pos = Pivot.position;
            var fwd = Pivot.forward;

            // Fire bullets
            var bulletTime = 1f / BulletsPerSecond;
            if (_timeAccumulator > bulletTime)
            {
                var bpos = pos + fwd * 0.5f;
                _timeAccumulator = 0;
                FlakManager.Spawn(bpos, BulletSpeed, Target);
                Statistics.ReportBulletFired();
            }
        }

        protected override void UpdateNotFacingTarget()
        {
            _timeAccumulator = 0;
        }
    }
}
