using System;
using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class BossController : MonoBehaviour
    {
        [SerializeField] private Damageable _damageable;
        [SerializeField] private TextMesh _healthLabel;
        [SerializeField] private int _collisionPenalty = 25;
        [SerializeField] private string _displayName = "TANK BOSS";

        public static event Action<BossController> BossSpawned;
        public static event Action<BossController> BossDefeated;
        public static event Action<BossController, float> BossHealthChanged;

        public string DisplayName => _displayName;
        public int Health => _damageable != null ? _damageable.Health : 0;
        public int MaxHealth { get; private set; }

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
        }

        public void Configure(int health, int collisionPenalty, string displayName)
        {
            MaxHealth = Mathf.Max(1, health);
            _collisionPenalty = Mathf.Max(0, collisionPenalty);
            _displayName = string.IsNullOrWhiteSpace(displayName) ? "TANK BOSS" : displayName;

            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
            _damageable.Configure(CombatTargetKind.Boss, MaxHealth, _healthLabel);
            UpdateLabel();
            BossSpawned?.Invoke(this);
            BossHealthChanged?.Invoke(this, 1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            CrowdManager crowd = other.GetComponentInParent<CrowdManager>();
            if (crowd == null || _damageable == null || !_damageable.IsAlive)
            {
                return;
            }

            crowd.Remove(_collisionPenalty);
            VfxManager.SpawnFloatingText("-" + _collisionPenalty, crowd.transform.position + Vector3.up * 2.3f, new Color(1f, 0.24f, 0.12f));
        }

        private void OnDamaged(Damageable damageable, int amount)
        {
            UpdateLabel();
            BossHealthChanged?.Invoke(this, Mathf.Clamp01(damageable.Health / (float)Mathf.Max(1, MaxHealth)));
        }

        private void OnDied(Damageable damageable)
        {
            UpdateLabel();
            VfxManager.SpawnFloatingText("BOSS DOWN", transform.position + Vector3.up * 2.8f, new Color(1f, 0.78f, 0.12f));
            BossHealthChanged?.Invoke(this, 0f);
            BossDefeated?.Invoke(this);
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

        private void UpdateLabel()
        {
            if (_healthLabel != null)
            {
                _healthLabel.text = _displayName + "\n" + Mathf.Max(0, Health);
            }
        }
    }
}
