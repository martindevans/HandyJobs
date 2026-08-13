using UnityEngine;

namespace Scenes.Auction
{
    public class Gun
        : BaseWeaponSystem
    {
        private Projectile _target;

        public Transform PivotTransform;
        public override Transform Pivot => PivotTransform;
        public BulletManager BulletManager;
        public ProjectileStatistics Statistics;

        public float BulletsPerSecond = 1;
        public float BulletSpeed = 10;
        public float RotationSpeed = 90;

        private float _timeAccumulator;

        private void OnEnable()
        {
            if (!BulletManager)
                BulletManager = GetComponentInParent<BulletManager>();
            if (!Statistics)
                Statistics = GetComponentInParent<ProjectileStatistics>();
        }

        public override void Assign(Projectile target)
        {
            _target = target;
        }

        private void Update()
        {
            if (_target)
            {
                // Turn to target
                var toTgt = _target.transform.position - Pivot.position;
                var rot = Quaternion.LookRotation(toTgt, Vector3.up);
                Pivot.rotation = Quaternion.RotateTowards(Pivot.rotation, rot, RotationSpeed * Time.deltaTime);

                // Check if we're facing target
                if (Vector3.Angle(Pivot.forward, toTgt) < 2)
                {
                    _timeAccumulator += Time.deltaTime;

                    var pos = Pivot.position;
                    var fwd = Pivot.forward;

                    // Fire bullets
                    var bulletTime = 1f / BulletsPerSecond;
                    if (_timeAccumulator > bulletTime)
                    {
                        var bpos = pos + fwd * 0.5f;
                        _timeAccumulator = 0;
                        BulletManager.Spawn(bpos, BulletSpeed, _target);
                        Statistics.ReportBulletFired();
                    }
                }
                else
                {
                    _timeAccumulator = 0;
                }
            }
        }
    }
}
