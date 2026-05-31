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

            if (Time.time < _nextFireTime)
            {
                return;
            }

            Damageable target = TargetRegistry.FindBestTarget(_aimOrigin != null ? _aimOrigin.position : transform.position, _tuning.targetRange, _tuning.targetLateralRange);
            if (target == null)
            {
                return;
            }

            float fireRateMultiplier = 1f;
            int damagePerSoldier = Mathf.Max(1, _tuning.baseDamage);
            if (_upgradeService != null)
            {
                damagePerSoldier = Mathf.RoundToInt(Mathf.Max(1f, _upgradeService.GetValue(UpgradeType.Damage)));
                fireRateMultiplier = Mathf.Max(1f, _upgradeService.GetValue(UpgradeType.FireRate));
            }

            float baseFireInterval = Mathf.Max(_tuning.minFireInterval, _tuning.baseFireInterval);
            float fireInterval = Mathf.Max(_tuning.minFireInterval, baseFireInterval / fireRateMultiplier);
            float targetMultiplier = GetTargetMultiplier(target, out bool critical);
            int armyScaledDamage = Mathf.Max(1, Mathf.RoundToInt(damagePerSoldier * Mathf.Max(1, _crowd.Count) * baseFireInterval * targetMultiplier));
            if (critical)
            {
                VfxManager.SpawnFloatingText("CRIT", target.AimPoint + Vector3.up * 0.65f, new Color(1f, 0.9f, 0.15f));
            }
            FireVisualBurst(target, armyScaledDamage);
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

        private void FireVisualBurst(Damageable target, int totalDamage)
        {
            if (_poolManager == null || _projectilePrefab == null || target == null)
            {
                target?.ApplyDamage(totalDamage);
                return;
            }

            int projectileCount = Mathf.Clamp(_crowd.Count / 8 + 1, 1, _tuning.projectileVisualBurst);
            int damagePerProjectile = Mathf.Max(1, Mathf.CeilToInt(totalDamage / (float)projectileCount));
            Vector3 origin = _aimOrigin != null ? _aimOrigin.position : transform.position + Vector3.up * 0.8f;
            _crowd.PlayShootFeedback(projectileCount + 1);
            VfxManager.Spawn(VfxCue.MuzzleFlash, origin + Vector3.forward * 0.35f);

            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 jitter = new Vector3(Random.Range(-0.45f, 0.45f), Random.Range(-0.05f, 0.25f), Random.Range(-0.35f, 0.1f));
                Projectile projectile = _poolManager.Get<Projectile>(_projectilePrefab, origin + jitter, Quaternion.identity);
                projectile.Fire(target, damagePerProjectile, _tuning.projectileSpeed, _tuning.projectileLifetime);
            }

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.Shoot);
            }
        }
    }
}
