using UnityEngine;

namespace Scenes.Auction
{
    public class BaseDefenceProjectile
        : MonoBehaviour
    {
        public Rigidbody RigidBody;
        public float DistanceFromOrigin;

        protected virtual void Update()
        {
            DistanceFromOrigin = RigidBody.position.magnitude;
        }
    }
}
