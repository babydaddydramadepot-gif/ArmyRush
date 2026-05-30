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

        private readonly List<SoldierUnitVisual> _units = new List<SoldierUnitVisual>();
        private int _unitCount;
        private int _healthPerUnit;
        private int _lastDisplayedUnitCount;

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

        public void Configure(int count, int healthPerUnit, GameObject unitPrefab, PoolManager poolManager)
        {
            _unitCount = Mathf.Max(1, count);
            _healthPerUnit = Mathf.Max(1, healthPerUnit);
            _enemyUnitPrefab = unitPrefab;
            _poolManager = poolManager;
            _lastDisplayedUnitCount = _unitCount;

            if (_damageable == null)
            {
                _damageable = GetComponent<Damageable>();
            }
            _damageable.Configure(CombatTargetKind.Enemy, _unitCount * _healthPerUnit, _countLabel);

            SyncUnits(_unitCount);
            UpdateLabel();
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
                crowd.Remove(remainingEnemies);
                _damageable.ApplyDamage(_damageable.Health);
            }
            else
            {
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
            }
            PlayHitFeedback(amount);
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
            VfxManager.SpawnFloatingText("CLEAR", transform.position + Vector3.up * 2.15f, new Color(1f, 0.85f, 0.18f));
            SyncUnits(0);
            gameObject.SetActive(false);
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
                _units[i].SetTargetLocalPosition(new Vector3(x, 0f, z), i);
            }
        }

        private void UpdateLabel()
        {
            if (_countLabel != null && _damageable != null)
            {
                _countLabel.text = Mathf.CeilToInt(_damageable.Health / (float)_healthPerUnit).ToString();
            }
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
    }
}
