using UnityEngine;

namespace ArmyRush
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float _hitDistance = 0.18f;

        private Damageable _target;
        private int _damage;
        private float _speed;
        private float _lifeRemaining;
        private PooledObject _pooledObject;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObject>();
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
                _target.ApplyDamage(_damage);
                if (ServiceLocator.TryGet(out AudioService audio))
                {
                    audio.Play(AudioCue.Hit);
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
