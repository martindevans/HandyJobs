using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Auction
{
    public class Projectile
        : MonoBehaviour
    {
        public Rigidbody RigidBody;
        public float Hitpoints;

        public GameObject PrefabOnDestroy;

        public ProjectileStatistics Statistics;

        private Transform _transform;
        public Vector3 Position => _transform.position;
        public float DistanceFromOrigin { get; private set; }

        private Rigidbody _body;
        public Vector3 Velocity => _body.linearVelocity;

        private readonly HashSet<Bullet> _bullets = new();
        public int AttachedBulletsCount => _bullets.Count;

        private void Awake()
        {
            _transform = transform;
            _body = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            var hitShip = collision.collider.gameObject.CompareTag(TagHandle.GetExistingTag("Player"));
            if (hitShip)
                Statistics.ReportHit();
            else if (collision.gameObject.TryGetComponent<Bullet>(out _))
                Statistics.ReportBulletInterception(collision.transform.position);

            DestroySelf();
        }

        private void Update()
        {
            DistanceFromOrigin = Position.magnitude;
            _bullets.RemoveWhere(DistanceIsGreater);
        }

        private bool DistanceIsGreater(Bullet bullet)
        {
            return bullet.DistanceFromOrigin > DistanceFromOrigin + 1;
        }

        public void LaserDamage(float damage)
        {
            Hitpoints -= damage;

            if (Hitpoints <= 0)
            {
                Statistics.ReportInterception(transform.position);
                DestroySelf();
            }
        }

        private void DestroySelf()
        {
            if (PrefabOnDestroy)
                Instantiate(PrefabOnDestroy, transform.position, transform.rotation);
            Destroy(gameObject);
        }

        public void AddBullet(Bullet go)
        {
            _bullets.Add(go);
        }

        public void RemoveBullet(Bullet go)
        {
            _bullets.Remove(go);
        }
    }
}
