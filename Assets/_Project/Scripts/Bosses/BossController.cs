using System;
using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class BossController : MonoBehaviour
    {
        [SerializeField] private Damageable _damageable;
        [SerializeField] private TextMesh _healthLabel;
        [SerializeField] private BossDefinition _definition;
        [SerializeField] private int _collisionPenalty = 25;
        [SerializeField] private string _displayName = "TANK BOSS";

        public static event Action<BossController> BossSpawned;
        public static event Action<BossController> BossDefeated;
        public static event Action<BossController, float> BossHealthChanged;

        public string DisplayName => _displayName;
        public int Health => _damageable != null ? _damageable.Health : 0;
        public int MaxHealth { get; private set; }
        public bool IsDefeated => _damageable == null || !_damageable.IsAlive;

        private CrowdManager _targetCrowd;
        private RunManager _runManager;
        private int _levelIndex = 1;
        private bool _engaged;
        private bool _attackWarningActive;
        private float _nextAttackTime;
        private float _attackResolveTime;
        private Vector3 _attackMarker;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
        }

        private void Update()
        {
            if (!_engaged || _targetCrowd == null || _targetCrowd.Count <= 0 || _damageable == null || !_damageable.IsAlive)
            {
                return;
            }

            if (_attackWarningActive)
            {
                if (Time.time >= _attackResolveTime)
                {
                    ResolveAttack();
                }
                return;
            }

            if (Time.time >= _nextAttackTime && IsCrowdInAttackWindow())
            {
                BeginAttack();
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
            _definition = null;
            _levelIndex = 1;
            _targetCrowd = null;
            _runManager = FindAnyObjectByType<RunManager>();
            MaxHealth = Mathf.Max(1, health);
            _collisionPenalty = Mathf.Max(0, collisionPenalty);
            _displayName = string.IsNullOrWhiteSpace(displayName) ? "TANK BOSS" : displayName;
            ApplyRuntimeConfig();
        }

        public void Configure(BossDefinition definition, int levelIndex, int healthOverride, CrowdManager targetCrowd)
        {
            _definition = definition;
            _levelIndex = Mathf.Max(1, levelIndex);
            _targetCrowd = targetCrowd;
            _runManager = FindAnyObjectByType<RunManager>();

            MaxHealth = _definition != null ? _definition.GetHealth(_levelIndex, healthOverride) : Mathf.Max(1, healthOverride);
            _collisionPenalty = _definition != null ? Mathf.Max(0, _definition.collisionPenalty) : _collisionPenalty;
            _displayName = _definition != null && !string.IsNullOrWhiteSpace(_definition.displayName) ? _definition.displayName : _displayName;

            ApplyRuntimeConfig();
        }

        private void ApplyRuntimeConfig()
        {
            _engaged = false;
            _attackWarningActive = false;
            _nextAttackTime = Time.time + 0.75f;

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

            if (!_engaged)
            {
                Engage(crowd);
            }

            crowd.Remove(_collisionPenalty);
            VfxManager.SpawnFloatingText("-" + _collisionPenalty, crowd.transform.position + Vector3.up * 2.3f, new Color(1f, 0.24f, 0.12f));
        }

        private void Engage(CrowdManager crowd)
        {
            _engaged = true;
            _targetCrowd = crowd;
            if (_runManager == null)
            {
                _runManager = FindAnyObjectByType<RunManager>();
            }
            _runManager?.PauseForCombat();
            _nextAttackTime = Time.time + 1.1f;

            VfxManager.SpawnFloatingText("BOSS FIGHT", transform.position + Vector3.up * 2.9f, new Color(1f, 0.78f, 0.12f));
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Warning);
            }
        }

        private void BeginAttack()
        {
            float warningDuration = _definition != null ? Mathf.Max(0.25f, _definition.warningDuration) : 0.7f;
            _attackWarningActive = true;
            _attackResolveTime = Time.time + warningDuration;
            _attackMarker = _targetCrowd.transform.position;
            _nextAttackTime = Time.time + (_definition != null ? Mathf.Max(0.8f, _definition.attackInterval) : 2.35f);

            string warning = _definition != null && !string.IsNullOrWhiteSpace(_definition.warningText) ? _definition.warningText : "CANNON";
            Color color = _definition != null ? _definition.warningColor : new Color(1f, 0.28f, 0.1f);
            VfxManager.SpawnFloatingText(warning, _attackMarker + Vector3.up * 2.4f, color);

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.BossAttack);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Warning);
            }
        }

        private void ResolveAttack()
        {
            _attackWarningActive = false;
            if (_targetCrowd == null || _targetCrowd.Count <= 0)
            {
                return;
            }

            Vector3 current = _targetCrowd.transform.position;
            Vector2 currentFlat = new Vector2(current.x, current.z);
            Vector2 markerFlat = new Vector2(_attackMarker.x, _attackMarker.z);
            float radius = _definition != null ? Mathf.Max(0.2f, _definition.attackRadius) : 1.45f;

            if (Vector2.Distance(currentFlat, markerFlat) <= radius)
            {
                int damage = _definition != null ? _definition.GetAttackDamage(_levelIndex) : 8;
                _targetCrowd.Remove(damage);
                VfxManager.SpawnFloatingText("HIT -" + damage, current + Vector3.up * 2.5f, new Color(1f, 0.18f, 0.1f));
                if (ServiceLocator.TryGet(out AudioService audio))
                {
                    audio.Play(AudioCue.Hit);
                }
                if (ServiceLocator.TryGet(out HapticsService haptics))
                {
                    haptics.Play(HapticCue.Medium);
                }
            }
            else
            {
                VfxManager.SpawnFloatingText("DODGED", current + Vector3.up * 2.4f, new Color(0.35f, 1f, 0.65f));
            }
        }

        private bool IsCrowdInAttackWindow()
        {
            if (_targetCrowd == null)
            {
                return false;
            }

            Vector3 crowdPosition = _targetCrowd.transform.position;
            float zDistance = transform.position.z - crowdPosition.z;
            float activationDistance = _definition != null ? Mathf.Max(1f, _definition.activationDistance) : 18f;
            float lateralRange = _definition != null ? Mathf.Max(0.5f, _definition.lateralRange) : 3.2f;
            return zDistance >= -1f && zDistance <= activationDistance && Mathf.Abs(transform.position.x - crowdPosition.x) <= lateralRange;
        }

        private void OnDamaged(Damageable damageable, int amount)
        {
            UpdateLabel();
            BossHealthChanged?.Invoke(this, Mathf.Clamp01(damageable.Health / (float)Mathf.Max(1, MaxHealth)));
        }

        private void OnDied(Damageable damageable)
        {
            UpdateLabel();
            VfxManager.Spawn(VfxCue.BossExplosion, transform.position + Vector3.up * 1.1f);
            VfxManager.SpawnFloatingText("BOSS DOWN", transform.position + Vector3.up * 2.8f, new Color(1f, 0.78f, 0.12f));
            BossHealthChanged?.Invoke(this, 0f);
            BossDefeated?.Invoke(this);
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.BossDefeat);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Success);
            }
            if (_runManager != null)
            {
                _runManager.ResumeFromCombat();
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
