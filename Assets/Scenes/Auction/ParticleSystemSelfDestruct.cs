using UnityEngine;

namespace Scenes.Auction
{
    public class ParticleSystemSelfDestruct
        : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        private void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (!_particleSystem)
                return;
            if (!gameObject.scene.isLoaded)
                return;
            if (!Application.isPlaying)
                return;

            if (!_particleSystem.IsAlive(true))
                Destroy(gameObject);
        }
    }
}
