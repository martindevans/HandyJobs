using JetBrains.Annotations;
using UnityEngine;

namespace Scenes.Auction
{
    public abstract class BaseWeaponSystem
        : MonoBehaviour
    {
        public abstract Transform Pivot { get; }

        public abstract void Assign([CanBeNull] Projectile target);
    }
}
