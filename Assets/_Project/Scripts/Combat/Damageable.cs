using System;
using UnityEngine;

namespace ArmyRush
{
    public sealed class Damageable : MonoBehaviour
    {
        [SerializeField] private CombatTargetKind _kind = CombatTargetKind.Enemy;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private TextMesh _label;
        [SerializeField] private bool _targetable = true;

        private int _health;
        private bool _alive;

        public event Action<Damageable, int> Damaged;
        public event Action<Damageable> Died;

        public CombatTargetKind Kind => _kind;
        public int Health => _health;
        public bool IsAlive => _alive && _health > 0 && gameObject.activeInHierarchy;
        public bool IsTargetable => _targetable;
        public Vector3 AimPoint => transform.position + Vector3.up * 0.8f;

        private void OnEnable()
        {
            if (_health <= 0)
            {
                _health = _maxHealth;
            }
            _alive = true;
            if (_targetable)
            {
                TargetRegistry.Register(this);
            }
            UpdateLabel();
        }

        private void OnDisable()
        {
            TargetRegistry.Unregister(this);
        }

        public void Configure(CombatTargetKind kind, int maxHealth, TextMesh label = null)
        {
            _kind = kind;
            _maxHealth = Mathf.Max(1, maxHealth);
            _label = label;
            WorldTextGuard.Clamp(_label);
            ResetHealth();
            RefreshTargetRegistration();
        }

        public void ResetHealth()
        {
            _health = _maxHealth;
            _alive = true;
            UpdateLabel();
            RefreshTargetRegistration();
        }

        public void SetTargetable(bool targetable)
        {
            _targetable = targetable;
            RefreshTargetRegistration();
        }

        public void ApplyDamage(int amount)
        {
            if (!_alive || amount <= 0)
            {
                return;
            }

            _health = Mathf.Max(0, _health - amount);
            Damaged?.Invoke(this, amount);
            VfxManager.Spawn(VfxCue.HitSpark, AimPoint);
            Color damageColor = _kind == CombatTargetKind.Obstacle || _kind == CombatTargetKind.Bonus ? new Color(1f, 0.8f, 0.18f) : new Color(1f, 0.28f, 0.18f);
            VfxManager.SpawnFloatingText("-" + amount, AimPoint + Vector3.up * 0.2f, damageColor);
            UpdateLabel();

            if (_health <= 0)
            {
                _alive = false;
                TargetRegistry.Unregister(this);
                Died?.Invoke(this);
            }
        }

        private void RefreshTargetRegistration()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_targetable && IsAlive)
            {
                TargetRegistry.Register(this);
            }
            else
            {
                TargetRegistry.Unregister(this);
            }
        }

        private void UpdateLabel()
        {
            if (_label != null)
            {
                if (_kind == CombatTargetKind.Enemy)
                {
                    WorldTextGuard.Clamp(_label);
                    return;
                }

                _label.text = Mathf.Max(0, _health).ToString();
                WorldTextGuard.Clamp(_label);
            }
        }
    }
}
