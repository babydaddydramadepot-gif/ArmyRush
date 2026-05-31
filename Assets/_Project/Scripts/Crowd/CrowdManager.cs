using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public sealed class CrowdManager : MonoBehaviour
    {
        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private GameObject _soldierPrefab;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private Transform _anchor;
        [SerializeField] private TextMesh _countLabel;

        private const float CountLabelPulseDuration = 0.34f;
        private const float CountLabelGainRise = 0.18f;
        private const float CountLabelLossDip = -0.08f;
        private const float CountLabelStartScale = 0.88f;

        private readonly List<SoldierUnitVisual> _soldiers = new List<SoldierUnitVisual>();
        private int _logicalCount;
        private int _shootFeedbackCursor;
        private bool _hasInitializedCount;
        private bool _countLabelPoseCached;
        private float _countLabelPulseTimer;
        private Vector3 _countLabelBaseScale = Vector3.one;
        private Vector3 _countLabelBaseLocalPosition;
        private Color _countLabelRestColor = Color.white;
        private Color _countLabelPulseColor = Color.white;
        private float _countLabelVerticalOffset;

        public event Action<int> CountChanged;
        public int Count => _logicalCount;
        public Transform Anchor => _anchor != null ? _anchor : transform;

        private void Awake()
        {
            CacheCountLabelPose();
        }

        private void Update()
        {
            UpdateCountLabelAnimation();
        }

        private void OnDisable()
        {
            ResetCountLabelAnimation();
        }

        public void Configure(GlobalTuning tuning, GameObject soldierPrefab, PoolManager poolManager, TextMesh countLabel)
        {
            _tuning = tuning;
            _soldierPrefab = soldierPrefab;
            _poolManager = poolManager;
            _countLabel = countLabel;
            _countLabelPoseCached = false;
            WorldTextGuard.Clamp(_countLabel);
            CacheCountLabelPose();
            if (_anchor == null)
            {
                _anchor = transform;
            }
        }

        public void PrewarmVisuals(int count)
        {
            if (_soldierPrefab == null || _poolManager == null || count <= 0)
            {
                return;
            }

            int visualCount = Mathf.Min(count, _tuning != null ? _tuning.maxVisualSoldiers : 120);
            _poolManager.Prewarm(_soldierPrefab, visualCount);
        }

        public void SetCount(int count)
        {
            int hardCap = _tuning != null ? _tuning.hardSoldierCap : 300;
            int previousCount = _logicalCount;
            _logicalCount = Mathf.Clamp(count, 0, hardCap);
            bool shouldAnimateLabel = _hasInitializedCount && previousCount != _logicalCount;
            SyncVisualCount();
            UpdateLabel();
            if (shouldAnimateLabel)
            {
                PlayCountLabelChangeAnimation(previousCount, _logicalCount);
            }
            SpawnCountChangeVfx(previousCount, _logicalCount);
            CountChanged?.Invoke(_logicalCount);
        }

        public void Add(int amount)
        {
            if (amount == 0)
            {
                return;
            }

            SetCount(_logicalCount + amount);
        }

        public void Multiply(int value)
        {
            SetCount(_logicalCount * Mathf.Max(1, value));
        }

        public void Divide(int value)
        {
            SetCount(Mathf.FloorToInt(_logicalCount / Mathf.Max(1f, value)));
        }

        public void Remove(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int previousCount = _logicalCount;
            SetCount(_logicalCount - amount);
            PlayHitFeedback(previousCount - _logicalCount);
        }

        public void PlayShootFeedback(int requestedUnits)
        {
            if (_soldiers.Count == 0 || requestedUnits <= 0)
            {
                return;
            }

            int feedbackCount = Mathf.Min(requestedUnits, _soldiers.Count, 16);
            int step = Mathf.Max(1, _soldiers.Count / feedbackCount);
            for (int i = 0; i < feedbackCount; i++)
            {
                int index = (_shootFeedbackCursor + i * step) % _soldiers.Count;
                _soldiers[index].PlayShootKick();
            }

            _shootFeedbackCursor = (_shootFeedbackCursor + 1) % _soldiers.Count;
        }

        public void PlayHitFeedback(int requestedUnits)
        {
            if (_soldiers.Count == 0 || requestedUnits <= 0)
            {
                return;
            }

            int feedbackCount = Mathf.Min(requestedUnits, _soldiers.Count, 24);
            int step = Mathf.Max(1, _soldiers.Count / feedbackCount);
            for (int i = 0; i < feedbackCount; i++)
            {
                int index = (_shootFeedbackCursor + i * step) % _soldiers.Count;
                float intensity = 1f + (i % 3) * 0.12f;
                _soldiers[index].PlayHitReaction(intensity);
            }
        }

        public void PlayVictoryCelebration()
        {
            if (_soldiers.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _soldiers.Count; i++)
            {
                float duration = 2.4f + (i % 5) * 0.08f;
                _soldiers[i].PlayVictoryCheer(duration, i * 0.53f);
            }
        }

        private void SyncVisualCount()
        {
            if (_soldierPrefab == null || _poolManager == null)
            {
                return;
            }

            int visualTarget = Mathf.Min(_logicalCount, _tuning != null ? _tuning.maxVisualSoldiers : 120);
            int previousVisualCount = _soldiers.Count;

            while (_soldiers.Count < visualTarget)
            {
                SoldierUnitVisual soldier = _poolManager.Get<SoldierUnitVisual>(_soldierPrefab, Anchor.position, Anchor.rotation, Anchor);
                soldier.Spawn();
                _soldiers.Add(soldier);
            }

            while (_soldiers.Count > visualTarget)
            {
                int lastIndex = _soldiers.Count - 1;
                SoldierUnitVisual soldier = _soldiers[lastIndex];
                _soldiers.RemoveAt(lastIndex);
                soldier.Despawn();
            }

            if (_soldiers.Count != previousVisualCount)
            {
                UpdateFormationTargets();
            }
        }

        private void UpdateFormationTargets()
        {
            int count = _soldiers.Count;
            if (count == 0)
            {
                return;
            }

            int maxColumns = Mathf.Max(3, _tuning != null ? _tuning.maxFormationColumns : 11);
            float spacingX = _tuning != null ? _tuning.soldierSpacingX : 0.48f;
            float spacingZ = _tuning != null ? _tuning.soldierSpacingZ : 0.46f;

            for (int i = 0; i < count; i++)
            {
                int row = Mathf.FloorToInt(i / (float)maxColumns);
                int columnsThisRow = Mathf.Min(maxColumns, count - row * maxColumns);
                int column = i - row * maxColumns;
                float centeredX = (column - (columnsThisRow - 1) * 0.5f) * spacingX;
                float rowArc = Mathf.Abs(centeredX) * 0.08f;
                Vector3 local = new Vector3(centeredX, 0f, -row * spacingZ - rowArc);
                _soldiers[i].SetTargetLocalPosition(local, i);
            }
        }

        private void UpdateLabel()
        {
            if (_countLabel != null)
            {
                _countLabel.text = _logicalCount.ToString();
                _countLabelRestColor = _logicalCount <= 5 ? new Color(1f, 0.35f, 0.25f) : Color.white;
                if (_countLabelPulseTimer <= 0f)
                {
                    _countLabel.color = _countLabelRestColor;
                }
                WorldTextGuard.Clamp(_countLabel);
                CacheCountLabelPose();
            }
        }

        private void PlayCountLabelChangeAnimation(int previousCount, int nextCount)
        {
            if (_countLabel == null || previousCount == nextCount)
            {
                return;
            }

            CacheCountLabelPose();
            bool gained = nextCount > previousCount;
            _countLabelPulseTimer = CountLabelPulseDuration;
            _countLabelPulseColor = gained ? new Color(0.22f, 1f, 0.64f) : new Color(1f, 0.32f, 0.16f);
            _countLabelVerticalOffset = gained ? CountLabelGainRise : CountLabelLossDip;
        }

        private void UpdateCountLabelAnimation()
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
            _countLabel.transform.localPosition = _countLabelBaseLocalPosition + Vector3.up * (_countLabelVerticalOffset * hop);
            _countLabel.color = Color.Lerp(_countLabelPulseColor, _countLabelRestColor, eased);

            if (_countLabelPulseTimer <= 0f)
            {
                ResetCountLabelAnimation();
            }
        }

        private void ResetCountLabelAnimation()
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

        private void SpawnCountChangeVfx(int previousCount, int nextCount)
        {
            if (!_hasInitializedCount)
            {
                _hasInitializedCount = true;
                return;
            }

            if (previousCount == nextCount)
            {
                return;
            }

            VfxCue cue = nextCount > previousCount ? VfxCue.CrowdGain : VfxCue.CrowdLoss;
            Vector3 position = Anchor.position + Vector3.up * 1.05f;
            VfxManager.Spawn(cue, position);
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(nextCount > previousCount ? AudioCue.CrowdGain : AudioCue.CrowdLoss);
            }
        }
    }
}
