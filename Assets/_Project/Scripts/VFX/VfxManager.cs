using UnityEngine;

namespace ArmyRush
{
    public enum VfxCue
    {
        MuzzleFlash,
        HitSpark,
        GatePositive,
        GateNegative,
        EnemyDefeat,
        CrowdGain,
        CrowdLoss,
        CoinBurst,
        ObstacleDebris,
        ObstacleExplosion,
        VictoryBurst,
        BossExplosion,
        SmokePuff,
        HeavySmoke
    }

    public sealed class VfxManager : MonoBehaviour
    {
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private GameObject _floatingTextPrefab;
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private GameObject _hitSparkPrefab;
        [SerializeField] private GameObject _gatePositivePrefab;
        [SerializeField] private GameObject _gateNegativePrefab;
        [SerializeField] private GameObject _enemyDefeatPrefab;
        [SerializeField] private GameObject _crowdGainPrefab;
        [SerializeField] private GameObject _crowdLossPrefab;
        [SerializeField] private GameObject _coinBurstPrefab;
        [SerializeField] private GameObject _obstacleDebrisPrefab;
        [SerializeField] private GameObject _obstacleExplosionPrefab;
        [SerializeField] private GameObject _victoryBurstPrefab;
        [SerializeField] private GameObject _bossExplosionPrefab;
        [SerializeField] private GameObject _smokePuffPrefab;
        [SerializeField] private GameObject _heavySmokePrefab;
        [SerializeField] private GameObject _bossTelegraphPrefab;

        private static VfxManager _active;
        private Material _runtimeSmokeMaterial;

        private void Awake()
        {
            _active = this;
            EnsureSmokePrefabs();
            EnsureEnemyDefeatPrefab();
            EnsureObstacleExplosionPrefab();
            EnsureBossTelegraphPrefab();
            if (_poolManager != null && _floatingTextPrefab != null)
            {
                _poolManager.Prewarm(_floatingTextPrefab, 24);
            }
            Prewarm(_muzzleFlashPrefab, 24);
            Prewarm(_hitSparkPrefab, 32);
            Prewarm(_gatePositivePrefab, 8);
            Prewarm(_gateNegativePrefab, 8);
            Prewarm(_enemyDefeatPrefab, 10);
            Prewarm(_crowdGainPrefab, 10);
            Prewarm(_crowdLossPrefab, 10);
            Prewarm(_coinBurstPrefab, 8);
            Prewarm(_obstacleDebrisPrefab, 8);
            Prewarm(_obstacleExplosionPrefab, 6);
            Prewarm(_victoryBurstPrefab, 4);
            Prewarm(_bossExplosionPrefab, 4);
            Prewarm(_smokePuffPrefab, 8);
            Prewarm(_heavySmokePrefab, 4);
            Prewarm(_bossTelegraphPrefab, 6);
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
            GameObject enemyDefeatPrefab,
            GameObject crowdGainPrefab,
            GameObject crowdLossPrefab,
            GameObject coinBurstPrefab,
            GameObject obstacleDebrisPrefab,
            GameObject obstacleExplosionPrefab,
            GameObject victoryBurstPrefab,
            GameObject bossExplosionPrefab,
            GameObject smokePuffPrefab,
            GameObject heavySmokePrefab,
            GameObject bossTelegraphPrefab = null)
        {
            _poolManager = poolManager;
            _floatingTextPrefab = floatingTextPrefab;
            _muzzleFlashPrefab = muzzleFlashPrefab;
            _hitSparkPrefab = hitSparkPrefab;
            _gatePositivePrefab = gatePositivePrefab;
            _gateNegativePrefab = gateNegativePrefab;
            _enemyDefeatPrefab = enemyDefeatPrefab;
            _crowdGainPrefab = crowdGainPrefab;
            _crowdLossPrefab = crowdLossPrefab;
            _coinBurstPrefab = coinBurstPrefab;
            _obstacleDebrisPrefab = obstacleDebrisPrefab;
            _obstacleExplosionPrefab = obstacleExplosionPrefab;
            _victoryBurstPrefab = victoryBurstPrefab;
            _bossExplosionPrefab = bossExplosionPrefab;
            _smokePuffPrefab = smokePuffPrefab;
            _heavySmokePrefab = heavySmokePrefab;
            _bossTelegraphPrefab = bossTelegraphPrefab;
        }

        public static void SpawnFloatingText(string text, Vector3 position, Color color)
        {
            if (_active == null || _active._poolManager == null || _active._floatingTextPrefab == null)
            {
                return;
            }

            FloatingText floatingText = _active._poolManager.Get<FloatingText>(_active._floatingTextPrefab, position, Quaternion.identity);
            floatingText.Play(text, color);
        }

        public static void Spawn(VfxCue cue, Vector3 position)
        {
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

        public static void SpawnBossTelegraph(Vector3 position, Color color, float radius, float duration)
        {
            if (_active == null || _active._poolManager == null)
            {
                return;
            }

            if (_active._bossTelegraphPrefab == null)
            {
                _active.EnsureBossTelegraphPrefab();
            }

            if (_active._bossTelegraphPrefab == null)
            {
                return;
            }

            PooledBossTelegraphVfx telegraph = _active._poolManager.Get<PooledBossTelegraphVfx>(_active._bossTelegraphPrefab, position, Quaternion.Euler(90f, 0f, 0f));
            if (telegraph != null)
            {
                telegraph.Play(color, radius, duration);
            }
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
                case VfxCue.EnemyDefeat:
                    return _enemyDefeatPrefab;
                case VfxCue.CrowdGain:
                    return _crowdGainPrefab;
                case VfxCue.CrowdLoss:
                    return _crowdLossPrefab;
                case VfxCue.CoinBurst:
                    return _coinBurstPrefab;
                case VfxCue.ObstacleDebris:
                    return _obstacleDebrisPrefab;
                case VfxCue.ObstacleExplosion:
                    return _obstacleExplosionPrefab != null ? _obstacleExplosionPrefab : _bossExplosionPrefab;
                case VfxCue.VictoryBurst:
                    return _victoryBurstPrefab;
                case VfxCue.BossExplosion:
                    return _bossExplosionPrefab;
                case VfxCue.SmokePuff:
                    return _smokePuffPrefab;
                case VfxCue.HeavySmoke:
                    return _heavySmokePrefab;
                default:
                    return null;
            }
        }

        private void EnsureObstacleExplosionPrefab()
        {
            if (_obstacleExplosionPrefab != null)
            {
                return;
            }

            GameObject root = new GameObject("PF_RuntimeObstacleExplosion");
            root.SetActive(false);
            root.transform.SetParent(transform, false);
            root.AddComponent<PooledObject>();
            ParticleSystem particles = root.AddComponent<ParticleSystem>();
            PooledParticleVfx pooledVfx = root.AddComponent<PooledParticleVfx>();

            ConfigureExplosionParticleSystem(particles, new Color(1f, 0.42f, 0.08f), 0.6f, 0.34f, 3.45f, 34, 0.28f, 0.82f, RuntimeSmokeMaterial);
            pooledVfx.Configure(new[] { particles }, 0.72f);
            _obstacleExplosionPrefab = root;
        }

        private void EnsureEnemyDefeatPrefab()
        {
            if (_enemyDefeatPrefab != null)
            {
                return;
            }

            GameObject root = new GameObject("PF_RuntimeEnemyDefeat");
            root.SetActive(false);
            root.transform.SetParent(transform, false);
            root.AddComponent<PooledObject>();
            ParticleSystem particles = root.AddComponent<ParticleSystem>();
            PooledParticleVfx pooledVfx = root.AddComponent<PooledParticleVfx>();

            ConfigureExplosionParticleSystem(particles, new Color(1f, 0.18f, 0.08f), 0.52f, 0.26f, 3.15f, 24, 0.18f, 0.52f, RuntimeSmokeMaterial);
            pooledVfx.Configure(new[] { particles }, 0.64f);
            _enemyDefeatPrefab = root;
        }

        private void EnsureSmokePrefabs()
        {
            if (_smokePuffPrefab == null)
            {
                _smokePuffPrefab = CreateRuntimeSmokePrefab("PF_RuntimeSmokePuff", new Color(0.36f, 0.39f, 0.42f, 0.56f), 1.05f, 0.72f, 0.68f, 12, 0.42f, 0.34f);
            }
            if (_heavySmokePrefab == null)
            {
                _heavySmokePrefab = CreateRuntimeSmokePrefab("PF_RuntimeHeavySmoke", new Color(0.25f, 0.26f, 0.28f, 0.62f), 1.42f, 1.05f, 0.52f, 20, 0.62f, 0.52f);
            }
        }

        private void EnsureBossTelegraphPrefab()
        {
            if (_bossTelegraphPrefab != null)
            {
                return;
            }

            GameObject root = new GameObject("PF_RuntimeBossTelegraph");
            root.SetActive(false);
            root.transform.SetParent(transform, false);
            root.AddComponent<PooledObject>();
            PooledBossTelegraphVfx telegraph = root.AddComponent<PooledBossTelegraphVfx>();
            ParticleSystem outerRing = CreateTelegraphParticleChild("OuterWarningRing", root.transform);
            ParticleSystem innerPulse = CreateTelegraphParticleChild("InnerPulse", root.transform);
            ParticleSystem sparks = CreateTelegraphParticleChild("WarningSparks", root.transform);
            telegraph.Configure(outerRing, innerPulse, sparks, 0.84f);
            _bossTelegraphPrefab = root;
        }

        private ParticleSystem CreateTelegraphParticleChild(string name, Transform parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            ParticleSystem particles = child.AddComponent<ParticleSystem>();
            ConfigureBossTelegraphParticleSystem(particles, RuntimeSmokeMaterial);
            return particles;
        }

        private static void ConfigureExplosionParticleSystem(
            ParticleSystem particles,
            Color color,
            float lifetime,
            float particleLifetime,
            float speed,
            int burstCount,
            float startSize,
            float radius,
            Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = lifetime;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = particleLifetime;
            main.startSpeed = speed;
            main.startSize = startSize;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.08f;
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(1, burstCount)) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            shape.randomDirectionAmount = 0.62f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(color, 0.22f),
                    new GradientColorKey(Color.Lerp(color, Color.black, 0.28f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.72f, 0.42f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.82f),
                new Keyframe(0.36f, 1.18f),
                new Keyframe(1f, 0.2f)));

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private GameObject CreateRuntimeSmokePrefab(string name, Color color, float lifetime, float particleLifetime, float speed, int burstCount, float startSize, float radius)
        {
            GameObject root = new GameObject(name);
            root.SetActive(false);
            root.transform.SetParent(transform, false);
            root.AddComponent<PooledObject>();
            ParticleSystem particles = root.AddComponent<ParticleSystem>();
            PooledParticleVfx pooledVfx = root.AddComponent<PooledParticleVfx>();

            ConfigureSmokeParticleSystem(particles, color, lifetime, particleLifetime, speed, burstCount, startSize, radius, RuntimeSmokeMaterial);
            pooledVfx.Configure(new[] { particles }, lifetime + 0.12f);

            return root;
        }

        private Material RuntimeSmokeMaterial
        {
            get
            {
                if (_runtimeSmokeMaterial == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                    if (shader == null)
                    {
                        shader = Shader.Find("Sprites/Default");
                    }

                    if (shader != null)
                    {
                        _runtimeSmokeMaterial = new Material(shader)
                        {
                            name = "MAT_RuntimeSmoke"
                        };
                    }
                }

                return _runtimeSmokeMaterial;
            }
        }

        public static void ConfigureSmokeParticleSystem(
            ParticleSystem particles,
            Color color,
            float lifetime,
            float particleLifetime,
            float speed,
            int burstCount,
            float startSize,
            float radius,
            Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = lifetime;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = particleLifetime;
            main.startSpeed = speed;
            main.startSize = startSize;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.05f;
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(1, burstCount)) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            shape.randomDirectionAmount = 0.18f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.Lerp(color, Color.white, 0.18f), 0.65f),
                    new GradientColorKey(Color.Lerp(color, Color.black, 0.12f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(color.a * 0.58f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.55f),
                new Keyframe(0.55f, 1.12f),
                new Keyframe(1f, 1.38f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        public static void ConfigureBossTelegraphParticleSystem(ParticleSystem particles, Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.84f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.84f;
            main.startSpeed = 0.02f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 0.42f, 0.1f, 0.78f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 48) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            shape.randomDirectionAmount = 0f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.78f, 0.18f), 0f),
                    new GradientColorKey(new Color(1f, 0.36f, 0.1f), 0.45f),
                    new GradientColorKey(new Color(0.7f, 0.04f, 0.02f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.76f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.72f),
                new Keyframe(0.46f, 1.08f),
                new Keyframe(1f, 0.12f)));

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
