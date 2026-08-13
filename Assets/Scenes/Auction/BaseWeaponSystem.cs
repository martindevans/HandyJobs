using JetBrains.Annotations;
using UnityEngine;

namespace Scenes.Auction
{
    public abstract class BaseWeaponSystem
        : MonoBehaviour
    {
        public Transform Pivot;

        public float RotationSpeed = 90;

        public Projectile Target { get; protected set; }

        public virtual void Assign([CanBeNull] Projectile target)
        {
            Target = target;
        }

        protected virtual void Update()
        {
            if (Target)
            {
                // Turn to target
                var toTgt = Target.transform.position - Pivot.position;
                var rot = Quaternion.LookRotation(toTgt, Vector3.up);
                Pivot.rotation = Quaternion.RotateTowards(Pivot.rotation, rot, RotationSpeed * Time.deltaTime);

                // Check if we're facing target
                if (Vector3.Angle(Pivot.forward, toTgt) < 2)
                    UpdateFacingTarget();
                else
                    UpdateNotFacingTarget();
            }
        }

        protected virtual void UpdateNotFacingTarget()
        {
        }

        protected virtual void UpdateFacingTarget()
        {
        }
    }
}
