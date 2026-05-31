using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class EnemyGroup : MonoBehaviour
    {
        [SerializeField] private GameObject _enemyUnitPrefab;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private TextMesh _countLabel;
        [SerializeField] private Damageable _damageable;
        [SerializeField] private int _coinReward = 10;

        private const float CountLabelPulseDuration = 0.22f;
        private const float CountLabelStartScale = 0.9f;
        private const float CountLabelLift = 0.1f;

        private readonly List<SoldierUnitVisual> _units = new List<SoldierUnitVisual>();
        private int _unitCount;
        private int _healthPerUnit;
        private int _lastDisplayedUnitCount;
        private RunManager _runManager;
        private int _attackFeedbackCursor;
        private float _nextAttackFeedbackTime;
        private bool _rewardClaimed;
        private Damageable _boundDamageable;
        private bool _countLabelPoseCached;
        private float _countLabelPulseTimer;
        private Vector3 _countLabelBaseScale = Vector3.one;
        private Vector3 _countLabelBaseLocalPosition;
        private Color _countLabelRestColor = Color.white;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;

            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
            CacheCountLabelPose();
        }

        private void Update()
        {
            UpdateCountLabelPulse();
        }

        private void OnEnable()
        {
            BindDamageableEvents();
        }

        private void OnDisable()
        {
            UnbindDamageableEvents();
            SyncUnits(0);
            ResetCountLabelPulse();
        }

        public void Configure(int count, int healthPerUnit, GameObject unitPrefab, PoolManager poolManager)
        {
            Configure(count, healthPerUnit, unitPrefab, poolManager, _coinReward, _runManager);
        }

        public void Configure(int count, int healthPerUnit, GameObject unitPrefab, PoolManager poolManager, int coinReward, RunManager runManager)
        {
            _unitCount = Mathf.Max(1, count);
            _healthPerUnit = Mathf.Max(1, healthPerUnit);
            _enemyUnitPrefab = unitPrefab;
            _poolManager = poolManager;
            _coinReward = Mathf.Max(0, coinReward);
            _runManager = runManager;
            _lastDisplayedUnitCount = _unitCount;
            _attackFeedbackCursor = 0;
            _nextAttackFeedbackTime = 0f;
            _rewardClaimed = false;
            _countLabelPoseCached = false;
            _countLabelPulseTimer = 0f;

            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
            _damageable.Configure(CombatTargetKind.Enemy, _unitCount * _healthPerUnit, _countLabel);
            BindDamageableEvents();

            SyncUnits(_unitCount);
            UpdateLabel();
        }

        private void BindDamageableEvents()
        {
            if (_boundDamageable == _damageable)
            {
                return;
            }

            UnbindDamageableEvents();
            if (_damageable == null)
            {
                return;
            }

            _damageable.Damaged += OnDamaged;
            _damageable.Died += OnDied;
            _boundDamageable = _damageable;
        }

        private void UnbindDamageableEvents()
        {
            if (_boundDamageable == null)
            {
                return;
            }

            _boundDamageable.Damaged -= OnDamaged;
            _boundDamageable.Died -= OnDied;
            _boundDamageable = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            CrowdManager crowd = other.GetComponentInParent<CrowdManager>();
            if (crowd == null || _damageable == null || !_damageable.IsAlive)
            {
                return;
            }

            int remainingEnemies = Mathf.CeilToInt(_damageable.Health / (float)_healthPerUnit);
            if (remainingEnemies <= 0)
            {
                ClearGroup();
                return;
            }

            if (crowd.Count > remainingEnemies)
            {
                PlayAttackFeedback(remainingEnemies, true);
                crowd.Remove(remainingEnemies);
                _damageable.ApplyDamage(_damageable.Health);
            }
            else if (_runManager != null && _runManager.TryApplyEarlyContactMercy(crowd, remainingEnemies, transform.position))
            {
                PlayAttackFeedback(Mathf.Min(remainingEnemies, _lastDisplayedUnitCount), true);
                _damageable.ApplyDamage(_damageable.Health);
            }
            else
            {
                PlayAttackFeedback(crowd.Count, true);
                crowd.Remove(crowd.Count);
                if (ServiceLocator.TryGet(out AudioService audio))
                {
                    audio.Play(AudioCue.Defeat);
                }
            }
        }

        private void OnDamaged(Damageable damageable, int amount)
        {
            int remainingUnits = Mathf.CeilToInt(damageable.Health / (float)_healthPerUnit);
            if (remainingUnits != _lastDisplayedUnitCount)
            {
                SyncUnits(remainingUnits);
                _lastDisplayedUnitCount = remainingUnits;
                PlayCountLabelDamagePulse();
            }
            PlayHitFeedback(amount);
            if (damageable.Health > 0)
            {
                PlayAttackFeedback(Mathf.CeilToInt(amount / (float)Mathf.Max(1, _healthPerUnit)), false);
            }
            UpdateLabel();
        }

        private void OnDied(Damageable damageable)
        {
            ClearGroup();
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.EnemyDefeat);
            }
        }

        private void ClearGroup()
        {
            VfxManager.Spawn(VfxCue.EnemyDefeat, transform.position + Vector3.up * 0.9f);
            VfxManager.SpawnFloatingText("CLEAR", transform.position + Vector3.up * 2.15f, new Color(1f, 0.85f, 0.18f));
            ClaimReward();
            SyncUnits(0);
            gameObject.SetActive(false);
        }

        private void ClaimReward()
        {
            if (_rewardClaimed || _coinReward <= 0 || _runManager == null)
            {
                return;
            }

            _rewardClaimed = true;
            Vector3 rewardPosition = transform.position + Vector3.up * 1.25f;
            _runManager.AddCombatCoins(_coinReward, rewardPosition);
            VfxManager.Spawn(VfxCue.CoinBurst, rewardPosition);
        }

        private void SyncUnits(int count)
        {
            int visualCount = Mathf.Min(count, 80);

            while (_units.Count < visualCount && _enemyUnitPrefab != null && _poolManager != null)
            {
                SoldierUnitVisual unit = _poolManager.Get<SoldierUnitVisual>(_enemyUnitPrefab, transform.position, transform.rotation, transform);
                unit.Spawn();
                _units.Add(unit);
            }

            while (_units.Count > visualCount)
            {
                int last = _units.Count - 1;
                SoldierUnitVisual unit = _units[last];
                _units.RemoveAt(last);
                unit.Despawn();
            }

            int maxColumns = 10;
            for (int i = 0; i < _units.Count; i++)
            {
                int row = Mathf.FloorToInt(i / (float)maxColumns);
                int columns = Mathf.Min(maxColumns, _units.Count - row * maxColumns);
                int column = i - row * maxColumns;
                float x = (column - (columns - 1) * 0.5f) * 0.45f;
                float z = row * 0.42f;
                _units[i].transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                _units[i].SetTargetLocalPosition(new Vector3(x, 0f, z), i);
            }
        }

        private void UpdateLabel()
        {
            if (_countLabel != null && _damageable != null)
            {
                _countLabel.text = Mathf.CeilToInt(_damageable.Health / (float)_healthPerUnit).ToString();
                if (_countLabelPulseTimer <= 0f)
                {
                    _countLabel.color = _countLabelRestColor;
                }
                WorldTextGuard.Clamp(_countLabel);
                CacheCountLabelPose();
            }
        }

        private void PlayCountLabelDamagePulse()
        {
            if (_countLabel == null)
            {
                return;
            }

            CacheCountLabelPose();
            _countLabelPulseTimer = CountLabelPulseDuration;
            _countLabel.color = new Color(1f, 0.3f, 0.16f);
        }

        private void UpdateCountLabelPulse()
        {
            if (_countLabel == null || _countLabelPulseTimer <= 0f)
            {
                return;
            }

            CacheCountLabelPose();
            _countLabelPulseTimer = Mathf.Max(0f, _countLabelPulseTimer - Time.deltaTime);
            float normalized = 1f - Mathf.Clamp01(_countLabelPulseTimer / CountLabelPulseDuration);
            float eased = EaseOutCubic(normalized);
            float hop = Mathf.Sin(normalized * Mathf.PI);
            float scale = Mathf.Lerp(CountLabelStartScale, 1f, eased);
            _countLabel.transform.localScale = _countLabelBaseScale * scale;
            _countLabel.transform.localPosition = _countLabelBaseLocalPosition + Vector3.up * (CountLabelLift * hop);
            _countLabel.color = Color.Lerp(new Color(1f, 0.3f, 0.16f), _countLabelRestColor, eased);

            if (_countLabelPulseTimer <= 0f)
            {
                ResetCountLabelPulse();
            }
        }

        private void ResetCountLabelPulse()
        {
            if (_countLabel == null || !_countLabelPoseCached)
            {
                return;
            }

            _countLabelPulseTimer = 0f;
            _countLabel.transform.localScale = _countLabelBaseScale;
            _countLabel.transform.localPosition = _countLabelBaseLocalPosition;
            _countLabel.color = _countLabelRestColor;
        }

        private void CacheCountLabelPose()
        {
            if (_countLabel == null || _countLabelPoseCached)
            {
                return;
            }

            WorldTextGuard.Clamp(_countLabel);
            Transform labelTransform = _countLabel.transform;
            _countLabelBaseScale = labelTransform.localScale;
            _countLabelBaseLocalPosition = labelTransform.localPosition;
            _countLabelRestColor = _countLabel.color;
            _countLabelPoseCached = true;
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - Mathf.Clamp01(value);
            return 1f - inverse * inverse * inverse;
        }

        private void PlayHitFeedback(int damageAmount)
        {
            if (_units.Count == 0 || damageAmount <= 0)
            {
                return;
            }

            int feedbackCount = Mathf.Min(_units.Count, Mathf.Max(1, Mathf.CeilToInt(damageAmount / (float)Mathf.Max(1, _healthPerUnit))));
            feedbackCount = Mathf.Min(feedbackCount, 18);
            int step = Mathf.Max(1, _units.Count / feedbackCount);
            for (int i = 0; i < feedbackCount; i++)
            {
                int index = (i * step) % _units.Count;
                _units[index].PlayHitReaction(1f + (i % 2) * 0.18f);
            }
        }

        private void PlayAttackFeedback(int requestedUnits, bool force)
        {
            if (_units.Count == 0 || requestedUnits <= 0)
            {
                return;
            }

            if (!force && Time.time < _nextAttackFeedbackTime)
            {
                return;
            }

            int feedbackCount = Mathf.Min(requestedUnits, _units.Count, 16);
            int step = Mathf.Max(1, _units.Count / feedbackCount);
            for (int i = 0; i < feedbackCount; i++)
            {
                int index = (_attackFeedbackCursor + i * step) % _units.Count;
                _units[index].PlayShootKick();
            }

            _attackFeedbackCursor = (_attackFeedbackCursor + 1) % _units.Count;
            _nextAttackFeedbackTime = Time.time + 0.28f;
            VfxManager.Spawn(VfxCue.MuzzleFlash, transform.position + Vector3.up * 1.05f - Vector3.forward * 0.7f);
        }
    }
}
