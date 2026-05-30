using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class ObstacleController : MonoBehaviour
    {
        [SerializeField] private Damageable _damageable;
        [SerializeField] private TextMesh _healthLabel;
        [SerializeField] private int _collisionPenalty = 8;
        [SerializeField] private int _coinReward = 12;
        [SerializeField] private ParticleSystem _destroyEffect;

        private RunManager _runManager;

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
            Configure(health, collisionPenalty, _coinReward, _runManager);
        }

        public void Configure(int health, int collisionPenalty, int coinReward, RunManager runManager)
        {
            _collisionPenalty = Mathf.Max(0, collisionPenalty);
            _coinReward = Mathf.Max(0, coinReward);
            _runManager = runManager;
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
            Vector3 impactCenter = transform.position + Vector3.up * 0.78f;
            VfxManager.Spawn(VfxCue.ObstacleExplosion, impactCenter);
            VfxManager.Spawn(VfxCue.ObstacleDebris, impactCenter);
            VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 0.65f);
            if (_collisionPenalty >= 14)
            {
                VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 0.72f);
            }

            if (_coinReward > 0 && _runManager != null)
            {
                Vector3 rewardPosition = transform.position + Vector3.up * 1.1f;
                _runManager.AddCombatCoins(_coinReward, rewardPosition);
                VfxManager.Spawn(VfxCue.CoinBurst, rewardPosition);
            }
            CameraFollowRig.Shake(CameraShakeCue.ObstacleBreak);

            if (_destroyEffect != null)
            {
                _destroyEffect.Play();
            }

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.ObstacleDestroyed);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Medium);
            }

            gameObject.SetActive(false);
        }
    }
}
