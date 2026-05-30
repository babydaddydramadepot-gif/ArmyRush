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

        private const float HitPulseDuration = 0.18f;
        private RunManager _runManager;
        private Vector3 _restScale;
        private Quaternion _restRotation;
        private float _hitPulseTimer;
        private int _configuredHealth = 1;
        private int _damageState;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }

            _restScale = transform.localScale;
            _restRotation = transform.localRotation;
        }

        private void Update()
        {
            if (_hitPulseTimer <= 0f)
            {
                return;
            }

            _hitPulseTimer = Mathf.Max(0f, _hitPulseTimer - Time.deltaTime);
            float normalized = _hitPulseTimer / HitPulseDuration;
            float wobble = Mathf.Sin(normalized * Mathf.PI * 4f) * normalized;
            transform.localScale = _restScale * (1f + normalized * 0.035f);
            transform.localRotation = _restRotation * Quaternion.Euler(wobble * 3.5f, 0f, -wobble * 4.5f);

            if (_hitPulseTimer <= 0f)
            {
                RestorePose();
            }
        }

        private void OnEnable()
        {
            _hitPulseTimer = 0f;
            _damageState = 0;
            RestorePose();
            if (_damageable != null)
            {
                _damageable.Damaged += OnDamaged;
                _damageable.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_damageable != null)
            {
                _damageable.Damaged -= OnDamaged;
                _damageable.Died -= OnDied;
            }
            RestorePose();
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
            _configuredHealth = Mathf.Max(1, health);
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
            _damageable.Configure(CombatTargetKind.Obstacle, _configuredHealth, _healthLabel);
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

        private void OnDamaged(Damageable damageable, int amount)
        {
            if (amount <= 0 || damageable == null || damageable.Health <= 0)
            {
                return;
            }

            _hitPulseTimer = HitPulseDuration;
            float healthPercent = Mathf.Clamp01(damageable.Health / (float)Mathf.Max(1, _configuredHealth));
            int nextState = healthPercent <= 0.4f ? 2 : healthPercent <= 0.7f ? 1 : 0;
            if (nextState <= _damageState)
            {
                return;
            }

            _damageState = nextState;
            Vector3 cuePosition = transform.position + Vector3.up * 0.95f;
            VfxManager.Spawn(nextState == 1 ? VfxCue.ObstacleDebris : VfxCue.SmokePuff, cuePosition);
            VfxManager.SpawnFloatingText(nextState == 1 ? "CRACKED" : "BREAKING", cuePosition + Vector3.up * 0.75f, new Color(1f, 0.62f, 0.16f));
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

        private void RestorePose()
        {
            transform.localScale = _restScale;
            transform.localRotation = _restRotation;
        }
    }
}
