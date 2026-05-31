using UnityEngine;

namespace ArmyRush
{
    public sealed class PlayerCombatController : MonoBehaviour
    {
        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private CrowdManager _crowd;
        [SerializeField] private RunManager _runManager;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _aimOrigin;

        private float _nextFireTime;
        private float _nextOpeningVolleyTime;
        private Damageable _currentTarget;
        private Damageable _closeAssistTarget;
        private UpgradeService _upgradeService;

        private void Start()
        {
            ServiceLocator.TryGet(out _upgradeService);
            if (_poolManager != null && _projectilePrefab != null && _tuning != null)
            {
                _poolManager.Prewarm(_projectilePrefab, 50);
            }
        }

        private void Update()
        {
            if (_runManager == null || (_runManager.State != RunState.Running && _runManager.State != RunState.CombatPaused && _runManager.State != RunState.FinishSequence) || _crowd == null || _crowd.Count <= 0 || _tuning == null)
            {
                return;
            }

            Vector3 origin = _aimOrigin != null ? _aimOrigin.position : transform.position + Vector3.up * 0.8f;
            Damageable target = TargetRegistry.FindBestTarget(origin, _tuning.targetRange, _tuning.targetLateralRange);
            if (target == null)
            {
                _currentTarget = null;
                _closeAssistTarget = null;
                return;
            }

            bool acquiredNewTarget = target != _currentTarget;
            if (acquiredNewTarget)
            {
                _currentTarget = target;
            }

            bool openingVolley = acquiredNewTarget
                && Time.time >= _nextOpeningVolleyTime
                && _tuning.openingVolleyDamageMultiplier > 1f;

            float fireRateMultiplier = 1f;
            int damagePerSoldier = Mathf.Max(1, _tuning.baseDamage);
            if (_upgradeService != null)
            {
                damagePerSoldier = Mathf.RoundToInt(Mathf.Max(1f, _upgradeService.GetValue(UpgradeType.Damage)));
                fireRateMultiplier = Mathf.Max(1f, _upgradeService.GetValue(UpgradeType.FireRate));
            }
            fireRateMultiplier *= _runManager != null ? _runManager.ActiveFireRateBoostMultiplier : 1f;

            float baseFireInterval = Mathf.Max(_tuning.minFireInterval, _tuning.baseFireInterval);
            float targetDistance = Mathf.Max(0f, target.transform.position.z - origin.z);
            bool closeRangeAssist = IsCloseRangeAssistActive(targetDistance, target);
            if (closeRangeAssist)
            {
                fireRateMultiplier *= Mathf.Max(1f, _tuning.closeRangeFireRateMultiplier);
            }
            float fireInterval = Mathf.Max(_tuning.minFireInterval, baseFireInterval / fireRateMultiplier);
            if (Time.time < _nextFireTime && !openingVolley)
            {
                return;
            }

            float targetMultiplier = GetTargetMultiplier(target, out bool critical);
            float openingMultiplier = openingVolley ? Mathf.Max(1f, _tuning.openingVolleyDamageMultiplier) : 1f;
            float urgencyMultiplier = closeRangeAssist ? Mathf.Max(1f, _tuning.closeRangeDamageMultiplier) : 1f;
            float runBoostMultiplier = _runManager != null ? _runManager.ActiveDamageBoostMultiplier : 1f;
            int armyScaledDamage = Mathf.Max(1, Mathf.RoundToInt(damagePerSoldier * Mathf.Max(1, _crowd.Count) * baseFireInterval * targetMultiplier * openingMultiplier * urgencyMultiplier * runBoostMultiplier));
            if (critical)
            {
                VfxManager.SpawnFloatingText("CRIT", target.AimPoint + Vector3.up * 0.65f, new Color(1f, 0.9f, 0.15f));
            }
            if (!closeRangeAssist)
            {
                _closeAssistTarget = null;
            }
            else if (target != _closeAssistTarget)
            {
                _closeAssistTarget = target;
                VfxManager.SpawnFloatingText("PUSH", target.AimPoint + Vector3.up * 0.85f, new Color(0.32f, 0.92f, 1f));
            }
            FireVisualBurst(target, armyScaledDamage, openingVolley, origin);
            if (openingVolley)
            {
                _nextOpeningVolleyTime = Time.time + Mathf.Max(0f, _tuning.openingVolleyCooldown);
            }
            _nextFireTime = Time.time + fireInterval;
        }

        public void Configure(GlobalTuning tuning, CrowdManager crowd, RunManager runManager, PoolManager poolManager, GameObject projectilePrefab, Transform aimOrigin)
        {
            _tuning = tuning;
            _crowd = crowd;
            _runManager = runManager;
            _poolManager = poolManager;
            _projectilePrefab = projectilePrefab;
            _aimOrigin = aimOrigin;
        }

        private float GetTargetMultiplier(Damageable target, out bool critical)
        {
            critical = false;
            if (_upgradeService == null || target == null)
            {
                return 1f;
            }

            float multiplier = 1f;
            if (target.Kind == CombatTargetKind.Boss)
            {
                multiplier *= Mathf.Max(1f, _upgradeService.GetValue(UpgradeType.BossDamage));
            }
            else if (target.Kind == CombatTargetKind.Obstacle)
            {
                multiplier *= Mathf.Max(1f, _upgradeService.GetValue(UpgradeType.ObstacleDamage));
            }

            float criticalChance = Mathf.Clamp01(_upgradeService.GetValue(UpgradeType.CriticalChance));
            if (criticalChance > 0f && Random.value <= criticalChance)
            {
                critical = true;
                multiplier *= Mathf.Max(1.25f, _upgradeService.GetValue(UpgradeType.CriticalDamage));
            }

            return multiplier;
        }

        private void FireVisualBurst(Damageable target, int totalDamage, bool openingVolley, Vector3 origin)
        {
            if (_poolManager == null || _projectilePrefab == null || target == null)
            {
                target?.ApplyDamage(totalDamage);
                return;
            }

            int extraProjectiles = openingVolley ? Mathf.Max(0, _tuning.openingVolleyExtraProjectiles) : 0;
            int projectileCount = Mathf.Clamp(_crowd.Count / 8 + 1 + extraProjectiles, 1, _tuning.projectileVisualBurst);
            int immediateDamage = CalculateImmediateDamage(totalDamage, openingVolley);
            if (immediateDamage > 0 && target.IsAlive)
            {
                target.ApplyDamage(immediateDamage);
            }

            int projectileDamage = Mathf.Max(1, totalDamage - immediateDamage);
            int damagePerProjectile = Mathf.Max(1, Mathf.CeilToInt(projectileDamage / (float)projectileCount));
            _crowd.PlayShootFeedback(projectileCount + 1);
            VfxManager.Spawn(VfxCue.MuzzleFlash, origin + Vector3.forward * 0.35f);

            if (target.IsAlive)
            {
                for (int i = 0; i < projectileCount; i++)
                {
                    Vector3 jitter = new Vector3(Random.Range(-0.45f, 0.45f), Random.Range(-0.05f, 0.25f), Random.Range(-0.35f, 0.1f));
                    Projectile projectile = _poolManager.Get<Projectile>(_projectilePrefab, origin + jitter, Quaternion.identity);
                    projectile.Fire(target, damagePerProjectile, _tuning.projectileSpeed, _tuning.projectileLifetime);
                }
            }

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.Shoot);
            }
        }

        private int CalculateImmediateDamage(int totalDamage, bool openingVolley)
        {
            if (totalDamage <= 1 || _tuning == null)
            {
                return 0;
            }

            float fraction = openingVolley
                ? Mathf.Max(_tuning.volleyImmediateDamageFraction, _tuning.openingVolleyImmediateDamageFraction)
                : _tuning.volleyImmediateDamageFraction;
            return Mathf.Clamp(Mathf.RoundToInt(totalDamage * Mathf.Clamp01(fraction)), 0, totalDamage - 1);
        }

        private bool IsCloseRangeAssistActive(float targetDistance, Damageable target)
        {
            if (_runManager == null || _tuning == null || target == null || target.Kind == CombatTargetKind.Bonus)
            {
                return false;
            }

            if (_runManager.CurrentLevelIndex > Mathf.Max(0, _tuning.earlyCloseRangeAssistLevelLimit))
            {
                return false;
            }

            return targetDistance <= Mathf.Max(0f, _tuning.closeRangeAssistDistance);
        }
    }
}
