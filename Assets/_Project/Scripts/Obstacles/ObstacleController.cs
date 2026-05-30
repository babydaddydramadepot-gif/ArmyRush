using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class ObstacleController : MonoBehaviour
    {
        [SerializeField] private Damageable _damageable;
        [SerializeField] private TextMesh _healthLabel;
        [SerializeField] private int _collisionPenalty = 8;
        [SerializeField] private ParticleSystem _destroyEffect;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
        }

        private void OnEnable()
        {
            if (_damageable != null)
            {
                _damageable.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_damageable != null)
            {
                _damageable.Died -= OnDied;
            }
        }

        public void Configure(int health, int collisionPenalty)
        {
            _collisionPenalty = Mathf.Max(0, collisionPenalty);
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
            _damageable.Configure(CombatTargetKind.Obstacle, health, _healthLabel);
        }

        private void OnTriggerEnter(Collider other)
        {
            CrowdManager crowd = other.GetComponentInParent<CrowdManager>();
            if (crowd == null || _damageable == null || !_damageable.IsAlive)
            {
                return;
            }

            crowd.Remove(_collisionPenalty);
            _damageable.ApplyDamage(_damageable.Health);
        }

        private void OnDied(Damageable damageable)
        {
            if (_destroyEffect != null)
            {
                _destroyEffect.Play();
            }

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.ObstacleDestroyed);
            }

            gameObject.SetActive(false);
        }
    }
}
