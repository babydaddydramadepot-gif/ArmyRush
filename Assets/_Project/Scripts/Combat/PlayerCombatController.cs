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
        private float _nextPowerSpikeVolleyTime;
        private Damageable _currentTarget;
        private Damageable _closeAssistTarget;
        private CrowdManager _subscribedCrowd;
        private UpgradeService _upgradeService;
        private bool _powerSpikeVolleyQueued;
        private int _lastCrowdCount;

        private void Start()
        {
            ServiceLocator.TryGet(out _upgradeService);
            SubscribeToCrowd();
            if (_poolManager != null && _projectilePrefab != null && _tuning != null)
            {
                _poolManager.Prewarm(_projectilePrefab, 50);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromCrowd();
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
                _powerSpikeVolleyQueued = false;
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
            bool powerSpikeVolley = !openingVolley
                && _powerSpikeVolleyQueued
                && Time.time >= _nextPowerSpikeVolleyTime
                && _tuning.powerSpikeVolleyDamageMultiplier > 1f;

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
            if (Time.time < _nextFireTime && !openingVolley && !powerSpikeVolley)
            {
                return;
            }

            float targetMultiplier = GetTargetMultiplier(target, out bool critical);
            float openingMultiplier = openingVolley ? Mathf.Max(1f, _tuning.openingVolleyDamageMultiplier) : 1f;
            float powerSpikeMultiplier = powerSpikeVolley ? Mathf.Max(1f, _tuning.powerSpikeVolleyDamageMultiplier) : 1f;
            float urgencyMultiplier = closeRangeAssist ? Mathf.Max(1f, _tuning.closeRangeDamageMultiplier) : 1f;
            float runBoostMultiplier = _runManager != null ? _runManager.ActiveDamageBoostMultiplier : 1f;
            int armyScaledDamage = Mathf.Max(1, Mathf.RoundToInt(damagePerSoldier * Mathf.Max(1, _crowd.Count) * baseFireInterval * targetMultiplier * openingMultiplier * powerSpikeMultiplier * urgencyMultiplier * runBoostMultiplier));
            if (critical)
            {
                VfxManager.SpawnFloatingText("CRIT", target.AimPoint + Vector3.up * 0.65f, new Color(1f, 0.9f, 0.15f));
            }
            if (powerSpikeVolley)
            {
                VfxManager.SpawnFloatingText("POWER", target.AimPoint + Vector3.up * 0.95f, new Color(0.25f, 0.95f, 1f));
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
            FireVisualBurst(target, armyScaledDamage, openingVolley, powerSpikeVolley, origin);
            if (openingVolley)
            {
                _nextOpeningVolleyTime = Time.time + Mathf.Max(0f, _tuning.openingVolleyCooldown);
            }
            if (openingVolley || powerSpikeVolley)
            {
                _powerSpikeVolleyQueued = false;
            }
            if (powerSpikeVolley)
            {
                _nextPowerSpikeVolleyTime = Time.time + Mathf.Max(0f, _tuning.powerSpikeVolleyCooldown);
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
            SubscribeToCrowd();
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

        private void FireVisualBurst(Damageable target, int totalDamage, bool openingVolley, bool powerSpikeVolley, Vector3 origin)
        {
            if (_poolManager == null || _projectilePrefab == null || target == null)
            {
                target?.ApplyDamage(totalDamage);
                return;
            }

            int extraProjectiles = openingVolley
                ? Mathf.Max(0, _tuning.openingVolleyExtraProjectiles)
                : powerSpikeVolley ? Mathf.Max(0, _tuning.powerSpikeVolleyExtraProjectiles) : 0;
            int projectileCount = Mathf.Clamp(_crowd.Count / 8 + 1 + extraProjectiles, 1, _tuning.projectileVisualBurst);
            int immediateDamage = CalculateImmediateDamage(totalDamage, openingVolley || powerSpikeVolley);
            if (immediateDamage > 0 && target.IsAlive)
            {
                target.ApplyDamage(immediateDamage);
            }

            int projectileDamage = Mathf.Max(1, totalDamage - immediateDamage);
            int damagePerProjectile = Mathf.Max(1, Mathf.CeilToInt(projectileDamage / (float)projectileCount));
            _crowd.PlayShootFeedback(projectileCount + 1);
            SpawnMuzzleFlashBurst(origin, projectileCount);

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
                int audioIntensity = projectileCount + (openingVolley || powerSpikeVolley ? 2 : 0);
                audio.PlayShootBurst(audioIntensity);
            }
        }

        private void SpawnMuzzleFlashBurst(Vector3 origin, int projectileCount)
        {
            int flashCount = Mathf.Clamp(projectileCount, 1, Mathf.Max(1, _tuning.muzzleFlashVisualBurst));
            Vector3 basePosition = origin + Vector3.forward * 0.35f;
            for (int i = 0; i < flashCount; i++)
            {
                Vector3 jitter = flashCount == 1
                    ? Vector3.zero
                    : new Vector3(Random.Range(-0.38f, 0.38f), Random.Range(-0.04f, 0.18f), Random.Range(-0.18f, 0.12f));
                VfxManager.Spawn(VfxCue.MuzzleFlash, basePosition + jitter);
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

        private void SubscribeToCrowd()
        {
            if (_subscribedCrowd == _crowd)
            {
                return;
            }

            UnsubscribeFromCrowd();
            if (_crowd == null)
            {
                return;
            }

            _subscribedCrowd = _crowd;
            _lastCrowdCount = _crowd.Count;
            _crowd.CountChanged += OnCrowdCountChanged;
        }

        private void UnsubscribeFromCrowd()
        {
            if (_subscribedCrowd == null)
            {
                return;
            }

            _subscribedCrowd.CountChanged -= OnCrowdCountChanged;
            _subscribedCrowd = null;
        }

        private void OnCrowdCountChanged(int count)
        {
            int previousCount = _lastCrowdCount;
            _lastCrowdCount = count;
            if (_tuning == null || previousCount <= 0 || count <= previousCount)
            {
                return;
            }

            int gained = count - previousCount;
            int minimumGain = Mathf.Max(1, _tuning.powerSpikeVolleyMinimumGain);
            bool meaningfulGrowth = gained >= minimumGain || count >= Mathf.CeilToInt(previousCount * 1.35f);
            if (!meaningfulGrowth || Time.time < _nextPowerSpikeVolleyTime)
            {
                return;
            }

            if (_runManager == null || (_runManager.State != RunState.Running && _runManager.State != RunState.CombatPaused && _runManager.State != RunState.FinishSequence))
            {
                return;
            }

            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                return;
            }

            _powerSpikeVolleyQueued = true;
        }
    }
}
