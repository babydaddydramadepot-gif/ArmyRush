using System;
using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class BossController : MonoBehaviour
    {
        private const string MainRotorAName = "MainRotor_A";
        private const string MainRotorBName = "MainRotor_B";
        private const string TailRotorName = "TailRotor";

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
        private float _motionTimeOffset;
        private Transform _mainRotorA;
        private Transform _mainRotorB;
        private Transform _tailRotor;
        private Quaternion _mainRotorARestLocalRotation;
        private Quaternion _mainRotorBRestLocalRotation;
        private Quaternion _tailRotorRestLocalRotation;

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
            if (_damageable == null || !_damageable.IsAlive)
            {
                return;
            }

            UpdatePatternMotion();

            if (!_engaged || _targetCrowd == null || _targetCrowd.Count <= 0)
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
                _damageable.SetTargetable(false);
                _damageable.Damaged += OnDamaged;
                _damageable.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_damageable != null)
            {
                _damageable.SetTargetable(false);
                _damageable.Damaged -= OnDamaged;
                _damageable.Died -= OnDied;
            }
            StopDefeatAnimation();
            RestoreRestPose();
        }

        public void Configure(int health, int collisionPenalty, string displayName, RunManager runManager = null)
        {
            _definition = null;
            _levelIndex = 1;
            _targetCrowd = null;
            _runManager = runManager;
            MaxHealth = Mathf.Max(1, health);
            _collisionPenalty = Mathf.Max(0, collisionPenalty);
            _displayName = string.IsNullOrWhiteSpace(displayName) ? "TANK BOSS" : displayName;
            ApplyRuntimeConfig();
        }

        public void Configure(BossDefinition definition, int levelIndex, int healthOverride, CrowdManager targetCrowd, RunManager runManager = null)
        {
            _definition = definition;
            _levelIndex = Mathf.Max(1, levelIndex);
            _targetCrowd = targetCrowd;
            _runManager = runManager;

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
            _motionTimeOffset = _levelIndex * 0.37f;
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
            _damageable.SetTargetable(false);
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
            _damageable?.SetTargetable(true);
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
            _attackMarker = GetAttackMarker();
            _nextAttackTime = Time.time + GetCurrentAttackInterval();

            string warning = _definition != null && !string.IsNullOrWhiteSpace(_definition.warningText) ? _definition.warningText : "CANNON";
            Color color = _definition != null ? _definition.warningColor : new Color(1f, 0.28f, 0.1f);
            VfxManager.SpawnFloatingText(warning, _attackMarker + Vector3.up * 2.4f, color);
            VfxManager.SpawnBossTelegraph(_attackMarker + Vector3.up * 0.08f, color, GetCurrentAttackRadius(), warningDuration);
            SpawnAttackWarningFeedback(GetCurrentPattern(), _attackMarker);

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(GetAttackAudioCue(GetCurrentPattern()));
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Warning);
            }
        }

        private void SpawnAttackWarningFeedback(BossAttackPattern pattern, Vector3 marker)
        {
            Vector3 bossCenter = transform.position + Vector3.up * 1.25f;
            switch (pattern)
            {
                case BossAttackPattern.MissileStrike:
                    VfxManager.Spawn(VfxCue.MuzzleFlash, bossCenter + Vector3.forward * 0.35f);
                    VfxManager.Spawn(VfxCue.SmokePuff, bossCenter + Vector3.up * 0.35f);
                    break;

                case BossAttackPattern.ShockwaveSlam:
                    VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 0.36f);
                    VfxManager.Spawn(VfxCue.HitSpark, marker + Vector3.up * 0.45f);
                    break;

                case BossAttackPattern.SuppressionBurst:
                    VfxManager.Spawn(VfxCue.MuzzleFlash, bossCenter + Vector3.back * 0.25f);
                    VfxManager.Spawn(VfxCue.HitSpark, marker + Vector3.up * 0.45f);
                    break;

                default:
                    VfxManager.Spawn(VfxCue.MuzzleFlash, bossCenter + Vector3.back * 0.45f);
                    break;
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
            float radius = GetCurrentAttackRadius();

            if (Vector2.Distance(currentFlat, markerFlat) <= radius)
            {
                int damage = GetCurrentAttackDamage();
                _targetCrowd.Remove(damage);
                VfxManager.SpawnFloatingText("HIT -" + damage, current + Vector3.up * 2.5f, new Color(1f, 0.18f, 0.1f));
                SpawnAttackImpactFeedback(_attackMarker, true);
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
                SpawnAttackImpactFeedback(_attackMarker, false);
                VfxManager.SpawnFloatingText("DODGED", current + Vector3.up * 2.4f, new Color(0.35f, 1f, 0.65f));
            }
        }

        private Vector3 GetAttackMarker()
        {
            Vector3 current = _targetCrowd != null ? _targetCrowd.transform.position : transform.position;
            BossAttackPattern pattern = _definition != null ? _definition.attackPattern : BossAttackPattern.CannonVolley;
            switch (pattern)
            {
                case BossAttackPattern.MissileStrike:
                    float leadDirection = Mathf.Abs(current.x) < 0.2f ? Mathf.Sign(Mathf.Sin(Time.time + _motionTimeOffset)) : -Mathf.Sign(current.x);
                    return current + Vector3.right * (leadDirection * 0.35f);

                case BossAttackPattern.ShockwaveSlam:
                    return new Vector3(transform.position.x, current.y, current.z + 0.45f);

                case BossAttackPattern.SuppressionBurst:
                    return current + Vector3.right * (Mathf.Sin(Time.time * 1.7f + _motionTimeOffset) * 0.25f);

                default:
                    return current;
            }
        }

        private void SpawnAttackImpactFeedback(Vector3 position, bool hit)
        {
            BossAttackPattern pattern = _definition != null ? _definition.attackPattern : BossAttackPattern.CannonVolley;
            Vector3 impact = position + Vector3.up * 0.35f;
            switch (pattern)
            {
                case BossAttackPattern.MissileStrike:
                    VfxManager.Spawn(VfxCue.ObstacleExplosion, impact);
                    VfxManager.Spawn(VfxCue.SmokePuff, impact + Vector3.up * 0.25f);
                    break;

                case BossAttackPattern.ShockwaveSlam:
                    VfxManager.Spawn(VfxCue.HeavySmoke, impact);
                    VfxManager.Spawn(VfxCue.HitSpark, impact + Vector3.up * 0.35f);
                    CameraFollowRig.Shake(hit ? CameraShakeCue.BossHit : CameraShakeCue.ObstacleBreak);
                    break;

                case BossAttackPattern.SuppressionBurst:
                    VfxManager.Spawn(VfxCue.HitSpark, impact + Vector3.up * 0.3f);
                    VfxManager.Spawn(VfxCue.SmokePuff, impact);
                    break;

                default:
                    VfxManager.Spawn(VfxCue.HitSpark, impact + Vector3.up * 0.35f);
                    break;
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
            ApplyPhaseEscalation(nextState);

            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Medium);
            }
        }

        private void ApplyPhaseEscalation(int phaseIndex)
        {
            if (_definition == null || _definition.attackPattern != BossAttackPattern.ShockwaveSlam || phaseIndex <= 0)
            {
                return;
            }

            _nextAttackTime = Mathf.Min(_nextAttackTime, Time.time + (phaseIndex == 1 ? 0.52f : 0.34f));
            string text = phaseIndex == 1 ? "PHASE 2" : "FINAL PHASE";
            VfxManager.SpawnFloatingText(text, transform.position + Vector3.up * 3.25f, phaseIndex == 1 ? new Color(1f, 0.84f, 0.18f) : new Color(1f, 0.24f, 0.12f));
            VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 0.85f);
            CameraFollowRig.Shake(phaseIndex == 1 ? CameraShakeCue.BossHit : CameraShakeCue.BossDefeat);

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.BossShockwaveAttack);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(phaseIndex == 1 ? HapticCue.Medium : HapticCue.Warning);
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
            _damageable?.SetTargetable(false);
            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }
            BossAttackPattern pattern = GetCurrentPattern();
            SpawnDefeatInitialFeedback(pattern);
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
            BossAttackPattern pattern = GetCurrentPattern();
            float duration = GetDefeatDuration(pattern);
            float resumeDelay = GetDefeatResumeDelay(pattern);
            bool resumed = false;
            bool smokeSpawned = false;
            bool secondarySmokeSpawned = false;
            bool crashImpactSpawned = false;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float eased = EaseInOutCubic(normalized);
                float shake = Mathf.Sin(normalized * Mathf.PI * 7f) * (1f - normalized);
                ApplyDefeatPose(normalized, eased, shake);

                if (_healthLabel != null)
                {
                    Color labelColor = _healthLabel.color;
                    labelColor.a = Mathf.Lerp(1f, 0f, eased);
                    _healthLabel.color = labelColor;
                }

                SpawnDefeatStageFeedback(pattern, normalized, ref smokeSpawned, ref secondarySmokeSpawned, ref crashImpactSpawned);

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

        private void ApplyDefeatPose(float normalized, float eased, float shake)
        {
            BossAttackPattern pattern = GetCurrentPattern();
            if (pattern == BossAttackPattern.MissileStrike)
            {
                float lateralFall = Mathf.Sin(normalized * Mathf.PI * 2.6f + _motionTimeOffset) * Mathf.Lerp(0.08f, 0.92f, eased);
                float forwardDrift = Mathf.Lerp(0f, 1.25f, eased);
                float verticalDrop = Mathf.Lerp(0f, -1.18f, eased) + Mathf.Sin(normalized * Mathf.PI * 4f) * (1f - normalized) * 0.12f;
                float spin = normalized * 470f + shake * 16f;
                transform.localPosition = _restLocalPosition + new Vector3(lateralFall + shake * 0.07f, verticalDrop, forwardDrift);
                transform.localRotation = _restLocalRotation * Quaternion.Euler(Mathf.Lerp(0f, 38f, eased) + shake * 6f, spin, Mathf.Lerp(0f, -76f, eased) + shake * 12f);
                transform.localScale = Vector3.Lerp(_restLocalScale * 1.02f, new Vector3(_restLocalScale.x * 0.92f, _restLocalScale.y * 0.72f, _restLocalScale.z * 1.08f), eased);
                SpinRotorVisuals(Mathf.Lerp(3.2f, 0.25f, eased) + Mathf.Abs(shake) * 0.45f);
                return;
            }

            if (pattern == BossAttackPattern.ShockwaveSlam)
            {
                float hydraulicDrop = Mathf.Sin(normalized * Mathf.PI * 3f) * (1f - normalized) * 0.08f;
                transform.localPosition = _restLocalPosition + new Vector3(shake * 0.045f, Mathf.Lerp(0f, -0.48f, eased) + hydraulicDrop, 0f);
                transform.localRotation = _restLocalRotation * Quaternion.Euler(Mathf.Lerp(0f, -24f, eased) + shake * 3f, Mathf.Lerp(0f, 16f, eased), Mathf.Lerp(0f, -12f, eased));
                transform.localScale = Vector3.Lerp(_restLocalScale * 1.06f, new Vector3(_restLocalScale.x * 1.12f, _restLocalScale.y * 0.72f, _restLocalScale.z * 1.08f), eased);
                return;
            }

            transform.localPosition = _restLocalPosition + new Vector3(shake * 0.08f, Mathf.Lerp(0f, -0.32f, eased), 0f);
            transform.localRotation = _restLocalRotation * Quaternion.Euler(Mathf.Lerp(0f, -68f, eased), 0f, Mathf.Lerp(0f, 9f, eased) + shake * 7f);
            transform.localScale = Vector3.Lerp(_restLocalScale * 1.04f, _restLocalScale * 0.9f, eased);
        }

        private void UpdateLabel()
        {
            if (_healthLabel != null)
            {
                _healthLabel.text = _displayName + "\n" + Mathf.Max(0, Health);
                WorldTextGuard.Clamp(_healthLabel);
            }
        }

        private void CaptureRestPose()
        {
            _restLocalPosition = transform.localPosition;
            _restLocalRotation = transform.localRotation;
            _restLocalScale = transform.localScale;
            CaptureRotorReferences();
        }

        private void RestoreRestPose()
        {
            transform.localPosition = _restLocalPosition;
            transform.localRotation = _restLocalRotation;
            transform.localScale = _restLocalScale;
            RestoreRotorPose();
        }

        private void UpdatePatternMotion()
        {
            if (_defeatRoutine != null)
            {
                return;
            }

            BossAttackPattern pattern = GetCurrentPattern();
            float time = Time.time + _motionTimeOffset;
            switch (pattern)
            {
                case BossAttackPattern.MissileStrike:
                    float strafe = Mathf.Sin(time * 1.35f) * 1.15f;
                    float hover = Mathf.Sin(time * 3.4f) * 0.08f;
                    transform.localPosition = _restLocalPosition + new Vector3(strafe, hover, 0f);
                    transform.localRotation = _restLocalRotation * Quaternion.Euler(0f, 0f, -strafe * 4.5f);
                    SpinRotorVisuals(3.4f);
                    break;

                case BossAttackPattern.ShockwaveSlam:
                    float phaseIntensity = 1f + _damageState * 0.28f;
                    float stomp = Mathf.Abs(Mathf.Sin(time * (1.55f + _damageState * 0.22f)));
                    float eased = stomp * stomp;
                    transform.localPosition = _restLocalPosition + Vector3.up * Mathf.Lerp(0.03f * phaseIntensity, -0.04f * phaseIntensity, eased);
                    transform.localRotation = _restLocalRotation * Quaternion.Euler(Mathf.Sin(time * 1.1f) * 1.8f * phaseIntensity, 0f, Mathf.Sin(time * 0.9f) * 1.3f * phaseIntensity);
                    break;

                case BossAttackPattern.SuppressionBurst:
                    transform.localRotation = _restLocalRotation * Quaternion.Euler(0f, Mathf.Sin(time * 1.8f) * 5f, 0f);
                    break;

                default:
                    transform.localRotation = _restLocalRotation * Quaternion.Euler(0f, Mathf.Sin(time * 0.9f) * 1.6f, 0f);
                    break;
            }
        }

        private float GetCurrentAttackInterval()
        {
            float interval = _definition != null ? Mathf.Max(0.8f, _definition.attackInterval) : 2.35f;
            if (_definition != null && _definition.attackPattern == BossAttackPattern.ShockwaveSlam)
            {
                interval *= _damageState == 0 ? 1f : _damageState == 1 ? 0.82f : 0.68f;
            }

            return Mathf.Max(0.55f, interval);
        }

        private int GetCurrentAttackDamage()
        {
            int damage = _definition != null ? _definition.GetAttackDamage(_levelIndex) : 8;
            if (_definition != null && _definition.attackPattern == BossAttackPattern.ShockwaveSlam)
            {
                damage = Mathf.RoundToInt(damage * (1f + _damageState * 0.18f));
            }

            return Mathf.Max(1, damage);
        }

        private float GetCurrentAttackRadius()
        {
            float radius = _definition != null ? Mathf.Max(0.2f, _definition.attackRadius) : 1.45f;
            if (_definition != null && _definition.attackPattern == BossAttackPattern.ShockwaveSlam)
            {
                radius += _damageState * 0.18f;
            }

            return radius;
        }

        private static AudioCue GetAttackAudioCue(BossAttackPattern pattern)
        {
            switch (pattern)
            {
                case BossAttackPattern.MissileStrike:
                    return AudioCue.BossMissileAttack;
                case BossAttackPattern.ShockwaveSlam:
                    return AudioCue.BossShockwaveAttack;
                case BossAttackPattern.SuppressionBurst:
                case BossAttackPattern.CannonVolley:
                    return AudioCue.BossCannonAttack;
                default:
                    return AudioCue.BossAttack;
            }
        }

        private BossAttackPattern GetCurrentPattern()
        {
            return _definition != null ? _definition.attackPattern : BossAttackPattern.CannonVolley;
        }

        private void SpawnDefeatInitialFeedback(BossAttackPattern pattern)
        {
            if (pattern == BossAttackPattern.MissileStrike)
            {
                VfxManager.Spawn(VfxCue.HitSpark, transform.position + Vector3.up * 1.55f + Vector3.forward * 0.35f);
                VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 1.45f + Vector3.right * 0.45f);
                VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 1.35f + Vector3.left * 0.35f);
                VfxManager.SpawnFloatingText("MAYDAY", transform.position + Vector3.up * 3.05f, new Color(1f, 0.42f, 0.12f));
                return;
            }

            VfxManager.Spawn(VfxCue.BossExplosion, transform.position + Vector3.up * 1.1f);
            VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 1.05f);
            VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 1.65f + Vector3.right * 0.75f);
            VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 1.65f + Vector3.left * 0.75f);
        }

        private void SpawnDefeatStageFeedback(BossAttackPattern pattern, float normalized, ref bool smokeSpawned, ref bool secondarySmokeSpawned, ref bool crashImpactSpawned)
        {
            if (pattern == BossAttackPattern.MissileStrike)
            {
                if (!secondarySmokeSpawned && normalized >= 0.22f)
                {
                    secondarySmokeSpawned = true;
                    VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 1.15f + Vector3.back * 0.45f);
                    CameraFollowRig.Shake(CameraShakeCue.ObstacleBreak);
                    if (ServiceLocator.TryGet(out HapticsService warningHaptics))
                    {
                        warningHaptics.Play(HapticCue.Warning);
                    }
                }

                if (!smokeSpawned && normalized >= 0.46f)
                {
                    smokeSpawned = true;
                    VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 0.85f + Vector3.right * 0.55f);
                    VfxManager.Spawn(VfxCue.SmokePuff, transform.position + Vector3.up * 1.25f + Vector3.left * 0.45f);
                    VfxManager.Spawn(VfxCue.HitSpark, transform.position + Vector3.up * 1.0f);
                }

                if (!crashImpactSpawned && normalized >= 0.72f)
                {
                    crashImpactSpawned = true;
                    VfxManager.Spawn(VfxCue.BossExplosion, transform.position + Vector3.up * 0.5f);
                    VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 0.45f);
                    CameraFollowRig.Shake(CameraShakeCue.BossDefeat);
                    if (ServiceLocator.TryGet(out AudioService audio))
                    {
                        audio.Play(AudioCue.BossCrash);
                    }
                    if (ServiceLocator.TryGet(out HapticsService haptics))
                    {
                        haptics.Play(HapticCue.Medium);
                    }
                }
                return;
            }

            if (!smokeSpawned && normalized >= 0.42f)
            {
                smokeSpawned = true;
                VfxManager.Spawn(VfxCue.HeavySmoke, transform.position + Vector3.up * 0.65f);
            }
        }

        private float GetDefeatDuration(BossAttackPattern pattern)
        {
            return pattern == BossAttackPattern.MissileStrike ? 1.16f : pattern == BossAttackPattern.ShockwaveSlam ? 0.92f : 0.82f;
        }

        private float GetDefeatResumeDelay(BossAttackPattern pattern)
        {
            return pattern == BossAttackPattern.MissileStrike ? 0.64f : pattern == BossAttackPattern.ShockwaveSlam ? 0.5f : 0.46f;
        }

        private void CaptureRotorReferences()
        {
            if (_mainRotorA == null)
            {
                _mainRotorA = transform.Find(MainRotorAName);
            }
            if (_mainRotorB == null)
            {
                _mainRotorB = transform.Find(MainRotorBName);
            }
            if (_tailRotor == null)
            {
                _tailRotor = transform.Find(TailRotorName);
            }

            if (_mainRotorA != null)
            {
                _mainRotorARestLocalRotation = _mainRotorA.localRotation;
            }
            if (_mainRotorB != null)
            {
                _mainRotorBRestLocalRotation = _mainRotorB.localRotation;
            }
            if (_tailRotor != null)
            {
                _tailRotorRestLocalRotation = _tailRotor.localRotation;
            }
        }

        private void RestoreRotorPose()
        {
            if (_mainRotorA != null)
            {
                _mainRotorA.localRotation = _mainRotorARestLocalRotation;
            }
            if (_mainRotorB != null)
            {
                _mainRotorB.localRotation = _mainRotorBRestLocalRotation;
            }
            if (_tailRotor != null)
            {
                _tailRotor.localRotation = _tailRotorRestLocalRotation;
            }
        }

        private void SpinRotorVisuals(float speedMultiplier)
        {
            float spin = (Time.time + _motionTimeOffset) * 720f * Mathf.Max(0.1f, speedMultiplier);
            if (_mainRotorA != null)
            {
                _mainRotorA.localRotation = _mainRotorARestLocalRotation * Quaternion.Euler(0f, spin, 0f);
            }
            if (_mainRotorB != null)
            {
                _mainRotorB.localRotation = _mainRotorBRestLocalRotation * Quaternion.Euler(0f, spin + 90f, 0f);
            }
            if (_tailRotor != null)
            {
                _tailRotor.localRotation = _tailRotorRestLocalRotation * Quaternion.Euler(0f, 0f, -spin * 1.45f);
            }
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
