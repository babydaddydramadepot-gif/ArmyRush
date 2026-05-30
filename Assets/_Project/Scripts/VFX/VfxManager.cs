using UnityEngine;

namespace ArmyRush
{
    public enum VfxCue
    {
        MuzzleFlash,
        HitSpark,
        GatePositive,
        GateNegative,
        CoinBurst,
        ObstacleDebris,
        VictoryBurst,
        BossExplosion
    }

    public sealed class VfxManager : MonoBehaviour
    {
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private GameObject _floatingTextPrefab;
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private GameObject _hitSparkPrefab;
        [SerializeField] private GameObject _gatePositivePrefab;
        [SerializeField] private GameObject _gateNegativePrefab;
        [SerializeField] private GameObject _coinBurstPrefab;
        [SerializeField] private GameObject _obstacleDebrisPrefab;
        [SerializeField] private GameObject _victoryBurstPrefab;
        [SerializeField] private GameObject _bossExplosionPrefab;

        private static VfxManager _active;

        private void Awake()
        {
            _active = this;
            if (_poolManager != null && _floatingTextPrefab != null)
            {
                _poolManager.Prewarm(_floatingTextPrefab, 24);
            }
            Prewarm(_muzzleFlashPrefab, 24);
            Prewarm(_hitSparkPrefab, 32);
            Prewarm(_gatePositivePrefab, 8);
            Prewarm(_gateNegativePrefab, 8);
            Prewarm(_coinBurstPrefab, 8);
            Prewarm(_obstacleDebrisPrefab, 8);
            Prewarm(_victoryBurstPrefab, 4);
            Prewarm(_bossExplosionPrefab, 4);
        }

        private void OnDestroy()
        {
            if (_active == this)
            {
                _active = null;
            }
        }

        public void Configure(
            PoolManager poolManager,
            GameObject floatingTextPrefab,
            GameObject muzzleFlashPrefab,
            GameObject hitSparkPrefab,
            GameObject gatePositivePrefab,
            GameObject gateNegativePrefab,
            GameObject coinBurstPrefab,
            GameObject obstacleDebrisPrefab,
            GameObject victoryBurstPrefab,
            GameObject bossExplosionPrefab)
        {
            _poolManager = poolManager;
            _floatingTextPrefab = floatingTextPrefab;
            _muzzleFlashPrefab = muzzleFlashPrefab;
            _hitSparkPrefab = hitSparkPrefab;
            _gatePositivePrefab = gatePositivePrefab;
            _gateNegativePrefab = gateNegativePrefab;
            _coinBurstPrefab = coinBurstPrefab;
            _obstacleDebrisPrefab = obstacleDebrisPrefab;
            _victoryBurstPrefab = victoryBurstPrefab;
            _bossExplosionPrefab = bossExplosionPrefab;
        }

        public static void SpawnFloatingText(string text, Vector3 position, Color color)
        {
            if (_active == null)
            {
                _active = FindAnyObjectByType<VfxManager>();
            }

            if (_active == null || _active._poolManager == null || _active._floatingTextPrefab == null)
            {
                return;
            }

            FloatingText floatingText = _active._poolManager.Get<FloatingText>(_active._floatingTextPrefab, position, Quaternion.identity);
            floatingText.Play(text, color);
        }

        public static void Spawn(VfxCue cue, Vector3 position)
        {
            if (_active == null)
            {
                _active = FindAnyObjectByType<VfxManager>();
            }

            if (_active == null || _active._poolManager == null)
            {
                return;
            }

            GameObject prefab = _active.GetPrefab(cue);
            if (prefab == null)
            {
                return;
            }

            PooledParticleVfx vfx = _active._poolManager.Get<PooledParticleVfx>(prefab, position, Quaternion.identity);
            vfx.Play();
        }

        private void Prewarm(GameObject prefab, int count)
        {
            if (_poolManager != null && prefab != null)
            {
                _poolManager.Prewarm(prefab, count);
            }
        }

        private GameObject GetPrefab(VfxCue cue)
        {
            switch (cue)
            {
                case VfxCue.MuzzleFlash:
                    return _muzzleFlashPrefab;
                case VfxCue.HitSpark:
                    return _hitSparkPrefab;
                case VfxCue.GatePositive:
                    return _gatePositivePrefab;
                case VfxCue.GateNegative:
                    return _gateNegativePrefab;
                case VfxCue.CoinBurst:
                    return _coinBurstPrefab;
                case VfxCue.ObstacleDebris:
                    return _obstacleDebrisPrefab;
                case VfxCue.VictoryBurst:
                    return _victoryBurstPrefab;
                case VfxCue.BossExplosion:
                    return _bossExplosionPrefab;
                default:
                    return null;
            }
        }
    }
}
