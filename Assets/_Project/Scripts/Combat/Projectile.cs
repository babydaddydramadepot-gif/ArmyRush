using UnityEngine;

namespace ArmyRush
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float _hitDistance = 0.18f;
        [SerializeField] private TrailRenderer _trail;

        private Damageable _target;
        private int _damage;
        private float _speed;
        private float _lifeRemaining;
        private PooledObject _pooledObject;
        private static Material _runtimeTrailMaterial;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObject>();
            EnsureTrail();
        }

        private void Update()
        {
            _lifeRemaining -= Time.deltaTime;
            if (_lifeRemaining <= 0f || _target == null || !_target.IsAlive)
            {
                Release();
                return;
            }

            Vector3 aimPoint = _target.AimPoint;
            Vector3 direction = (aimPoint - transform.position).normalized;
            transform.position += direction * (_speed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            if (Vector3.Distance(transform.position, aimPoint) <= _hitDistance)
            {
                CombatTargetKind targetKind = _target.Kind;
                _target.ApplyDamage(_damage);
                if (ServiceLocator.TryGet(out AudioService audio))
                {
                    audio.Play(targetKind == CombatTargetKind.Obstacle ? AudioCue.ObstacleDamage : AudioCue.Hit);
                }
                Release();
            }
        }

        public void Fire(Damageable target, int damage, float speed, float lifetime)
        {
            _target = target;
            _damage = Mathf.Max(1, damage);
            _speed = speed;
            _lifeRemaining = lifetime;
            ResetTrail(true);
        }

        private void Release()
        {
            ResetTrail(false);
            _target = null;
            if (_pooledObject != null)
            {
                _pooledObject.Release();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void EnsureTrail()
        {
            if (_trail == null)
            {
                _trail = GetComponent<TrailRenderer>();
            }

            if (_trail == null)
            {
                _trail = gameObject.AddComponent<TrailRenderer>();
            }

            _trail.time = 0.09f;
            _trail.minVertexDistance = 0.035f;
            _trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.11f), new Keyframe(1f, 0f));
            _trail.startColor = new Color(1f, 0.88f, 0.18f, 0.78f);
            _trail.endColor = new Color(1f, 0.38f, 0.08f, 0f);
            _trail.alignment = LineAlignment.View;
            _trail.numCapVertices = 2;
            _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _trail.receiveShadows = false;
            _trail.material = RuntimeTrailMaterial;
            ResetTrail(false);
        }

        private void ResetTrail(bool emitting)
        {
            if (_trail == null)
            {
                return;
            }

            _trail.emitting = emitting;
            _trail.Clear();
        }

        private static Material RuntimeTrailMaterial
        {
            get
            {
                if (_runtimeTrailMaterial == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                    if (shader == null)
                    {
                        shader = Shader.Find("Sprites/Default");
                    }

                    if (shader == null)
                    {
                        return null;
                    }

                    _runtimeTrailMaterial = new Material(shader)
                    {
                        name = "MAT_RuntimeProjectileTrail"
                    };
                    _runtimeTrailMaterial.color = new Color(1f, 0.78f, 0.08f, 0.72f);
                }

                return _runtimeTrailMaterial;
            }
        }
    }
}
