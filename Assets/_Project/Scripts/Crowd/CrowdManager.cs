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

        private readonly List<SoldierUnitVisual> _soldiers = new List<SoldierUnitVisual>();
        private int _logicalCount;
        private int _shootFeedbackCursor;
        private bool _hasInitializedCount;

        public event Action<int> CountChanged;
        public int Count => _logicalCount;
        public Transform Anchor => _anchor != null ? _anchor : transform;

        private void Update()
        {
            if (_countLabel != null)
            {
                _countLabel.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            }
        }

        public void Configure(GlobalTuning tuning, GameObject soldierPrefab, PoolManager poolManager, TextMesh countLabel)
        {
            _tuning = tuning;
            _soldierPrefab = soldierPrefab;
            _poolManager = poolManager;
            _countLabel = countLabel;
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
            SyncVisualCount();
            UpdateLabel();
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
                _countLabel.color = _logicalCount <= 5 ? new Color(1f, 0.35f, 0.25f) : Color.white;
            }
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
