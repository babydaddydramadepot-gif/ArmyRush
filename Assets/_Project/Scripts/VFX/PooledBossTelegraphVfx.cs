using UnityEngine;

namespace ArmyRush
{
    public sealed class PooledBossTelegraphVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _outerRing;
        [SerializeField] private ParticleSystem _innerPulse;
        [SerializeField] private ParticleSystem _sparkBurst;
        [SerializeField] private float _defaultDuration = 0.8f;

        private PooledObject _pooledObject;
        private float _remaining;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObject>();
            if (_outerRing == null || _innerPulse == null || _sparkBurst == null)
            {
                ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(true);
                if (_outerRing == null && systems.Length > 0)
                {
                    _outerRing = systems[0];
                }
                if (_innerPulse == null && systems.Length > 1)
                {
                    _innerPulse = systems[1];
                }
                if (_sparkBurst == null && systems.Length > 2)
                {
                    _sparkBurst = systems[2];
                }
            }
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                Release();
            }
        }

        public void Configure(ParticleSystem outerRing, ParticleSystem innerPulse, ParticleSystem sparkBurst, float defaultDuration)
        {
            _outerRing = outerRing;
            _innerPulse = innerPulse;
            _sparkBurst = sparkBurst;
            _defaultDuration = Mathf.Max(0.18f, defaultDuration);
        }

        public void Play(Color warningColor, float radius, float duration)
        {
            float safeRadius = Mathf.Max(0.35f, radius);
            float safeDuration = Mathf.Max(0.18f, duration > 0f ? duration : _defaultDuration);
            _remaining = safeDuration + 0.18f;
            transform.localScale = Vector3.one;

            ConfigureRing(_outerRing, warningColor, safeRadius, safeDuration, 76, 0.09f, 0.04f);
            ConfigureRing(_innerPulse, Color.Lerp(warningColor, Color.white, 0.34f), safeRadius * 0.58f, safeDuration * 0.92f, 42, 0.14f, 0.02f);
            ConfigureSparkBurst(_sparkBurst, warningColor, safeRadius, safeDuration);

            PlaySystem(_outerRing);
            PlaySystem(_innerPulse);
            PlaySystem(_sparkBurst);
        }

        private static void ConfigureRing(ParticleSystem particles, Color color, float radius, float duration, int count, float startSize, float speed)
        {
            if (particles == null)
            {
                return;
            }

            ParticleSystem.MainModule main = particles.main;
            main.duration = duration;
            main.startLifetime = duration;
            main.startSpeed = speed;
            main.startSize = startSize;
            main.startColor = color;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(count, 1, 160)) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.randomDirectionAmount = 0f;

            ApplyColorFade(particles, color, 0.76f);
            ApplySizeCurve(particles, 0.72f, 1.08f, 0.12f);
        }

        private static void ConfigureSparkBurst(ParticleSystem particles, Color color, float radius, float duration)
        {
            if (particles == null)
            {
                return;
            }

            ParticleSystem.MainModule main = particles.main;
            main.duration = duration;
            main.startLifetime = Mathf.Min(0.34f, duration);
            main.startSpeed = 0.48f;
            main.startSize = 0.07f;
            main.startColor = Color.Lerp(color, Color.white, 0.26f);

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(duration * 0.45f, (short)18) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius * 0.42f;
            shape.randomDirectionAmount = 0.5f;

            ApplyColorFade(particles, color, 0.88f);
            ApplySizeCurve(particles, 0.46f, 1.22f, 0.05f);
        }

        private static void ApplyColorFade(ParticleSystem particles, Color color, float middleAlpha)
        {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.Lerp(color, Color.white, 0.18f), 0f),
                    new GradientColorKey(color, 0.45f),
                    new GradientColorKey(Color.Lerp(color, Color.black, 0.18f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(middleAlpha, 0.22f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;
        }

        private static void ApplySizeCurve(ParticleSystem particles, float start, float middle, float end)
        {
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, start),
                new Keyframe(0.46f, middle),
                new Keyframe(1f, end)));
        }

        private static void PlaySystem(ParticleSystem particles)
        {
            if (particles == null)
            {
                return;
            }

            particles.Clear(true);
            particles.Play(true);
        }

        private void Release()
        {
            if (_pooledObject != null)
            {
                _pooledObject.Release();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
