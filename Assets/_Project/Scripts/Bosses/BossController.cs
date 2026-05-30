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
        private Collider _triggerCollider;
        private int _levelIndex = 1;
        private bool _engaged;
        private bool _attackWarningActive;
        private float _nextAttackTime;
        private float _attackResolveTime;
        private Vector3 _attackMarker;
        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation;
        private Vector3 _restLocalScale;
        private Coroutine _defeatRoutine;
        private int _damageState;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
            _triggerCollider.isTrigger = true;
            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
            CaptureRestPose();
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
            StopDefeatAnimation();
            RestoreRestPose();
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
            StopDefeatAnimation();
            CaptureRestPose();
            RestoreRestPose();
            _engaged = false;
            _attackWarningActive = false;
            _nextAttackTime = Time.time + 0.75f;
            _damageState = 0;
            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = true;
            }
            if (_healthLabel != null)
            {
                _healthLabel.gameObject.SetActive(true);
                _healthLabel.color = Color.white;
            }

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
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.BossIntro);
            }
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
                CameraFollowRig.Shake(CameraShakeCue.BossHit);
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
            float healthPercent = Mathf.Clamp01(damageable.Health / (float)Mathf.Max(1, MaxHealth));
            BossHealthChanged?.Invoke(this, healthPercent);
            UpdateDamageState(healthPercent);
        }

        private void UpdateDamageState(float healthPercent)
        {
            int nextState = healthPercent <= 0.33f ? 2 : healthPercent <= 0.66f ? 1 : 0;
            if (nextState <= _damageState)
            {
                return;
            }

            _damageState = nextState;
            Vector3 smokePosition = transform.position + Vector3.up * (nextState == 1 ? 1.25f : 1.55f);
            VfxManager.Spawn(nextState == 1 ? VfxCue.SmokePuff : VfxCue.HeavySmoke, smokePosition);
            VfxManager.Spawn(VfxCue.HitSpark, smokePosition + Vector3.forward * 0.35f);
            VfxManager.SpawnFloatingText(nextState == 1 ? "ARMOR CRACKED" : "CRITICAL DAMAGE", transform.position + Vector3.up * 2.75f, new Color(1f, 0.62f, 0.16f));
            CameraFollowRig.Shake(CameraShakeCue.BossHit);

            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Medium);
            }
        }

        private void OnDied(Damageable damageable)
        {
            if (_defeatRoutine != null)
            {
                return;
            }

            UpdateLabel();
            _engaged = false;
            _attackWarningActive = false;
            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }
            VfxManager.Spawn(VfxCue.BossExplosion, transform.position + Vector3.up * 1.1f);
            VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 1.05f);
            VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 1.65f + Vector3.right * 0.75f);
            VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 1.65f + Vector3.left * 0.75f);
            VfxManager.SpawnFloatingText("BOSS DOWN", transform.position + Vector3.up * 2.8f, new Color(1f, 0.78f, 0.12f));
            CameraFollowRig.Shake(CameraShakeCue.BossDefeat);
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
            _defeatRoutine = StartCoroutine(PlayDefeatAnimation());
        }

        private System.Collections.IEnumerator PlayDefeatAnimation()
        {
            const float duration = 0.82f;
            const float resumeDelay = 0.46f;
            bool resumed = false;
            bool smokeSpawned = false;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float eased = EaseInOutCubic(normalized);
                float shake = Mathf.Sin(normalized * Mathf.PI * 7f) * (1f - normalized);
                transform.localPosition = _restLocalPosition + new Vector3(shake * 0.08f, Mathf.Lerp(0f, -0.32f, eased), 0f);
                transform.localRotation = _restLocalRotation * Quaternion.Euler(Mathf.Lerp(0f, -68f, eased), 0f, Mathf.Lerp(0f, 9f, eased) + shake * 7f);
                transform.localScale = Vector3.Lerp(_restLocalScale * 1.04f, _restLocalScale * 0.9f, eased);

                if (_healthLabel != null)
                {
                    Color labelColor = _healthLabel.color;
                    labelColor.a = Mathf.Lerp(1f, 0f, eased);
                    _healthLabel.color = labelColor;
                }

                if (!smokeSpawned && normalized >= 0.42f)
                {
                    smokeSpawned = true;
                    VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 0.65f);
                }

                if (!resumed && elapsed >= resumeDelay)
                {
                    resumed = true;
                    ResumeRunAfterDefeat();
                }

                yield return null;
            }

            if (!resumed)
            {
                ResumeRunAfterDefeat();
            }
            if (_healthLabel != null)
            {
                _healthLabel.gameObject.SetActive(false);
            }
            _defeatRoutine = null;
            gameObject.SetActive(false);
        }

        private void ResumeRunAfterDefeat()
        {
            if (_runManager != null)
            {
                _runManager.ResumeFromCombat();
            }
        }

        private void UpdateLabel()
        {
            if (_healthLabel != null)
            {
                _healthLabel.text = _displayName + "\n" + Mathf.Max(0, Health);
            }
        }

        private void CaptureRestPose()
        {
            _restLocalPosition = transform.localPosition;
            _restLocalRotation = transform.localRotation;
            _restLocalScale = transform.localScale;
        }

        private void RestoreRestPose()
        {
            transform.localPosition = _restLocalPosition;
            transform.localRotation = _restLocalRotation;
            transform.localScale = _restLocalScale;
        }

        private void StopDefeatAnimation()
        {
            if (_defeatRoutine == null)
            {
                return;
            }

            StopCoroutine(_defeatRoutine);
            _defeatRoutine = null;
        }

        private static float EaseInOutCubic(float value)
        {
            if (value < 0.5f)
            {
                return 4f * value * value * value;
            }

            float inverse = -2f * value + 2f;
            return 1f - inverse * inverse * inverse * 0.5f;
        }
    }
}
