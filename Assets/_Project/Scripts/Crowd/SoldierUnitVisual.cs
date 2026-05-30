using UnityEngine;

namespace ArmyRush
{
    public sealed class SoldierUnitVisual : MonoBehaviour
    {
        [SerializeField] private Transform _bodyRoot;
        [SerializeField] private Transform _weaponRoot;
        [SerializeField] private float _moveLerp = 18f;
        [SerializeField] private float _bobAmplitude = 0.045f;
        [SerializeField] private float _bobSpeed = 9f;
        [SerializeField] private float _deathDuration = 0.34f;

        private Vector3 _targetLocalPosition;
        private Vector3 _deathStartPosition;
        private Vector3 _deathVelocity;
        private int _formationIndex;
        private PooledObject _pooledObject;
        private float _spawnScale;
        private float _shootKickTimer;
        private float _deathTimer;
        private float _deathSpin;
        private bool _isDespawning;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObject>();
            if (_bodyRoot == null)
            {
                _bodyRoot = transform;
            }
        }

        private void Update()
        {
            if (_isDespawning)
            {
                UpdateDeathAnimation();
                return;
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetLocalPosition, 1f - Mathf.Exp(-_moveLerp * Time.deltaTime));
            float bob = Mathf.Sin(Time.time * _bobSpeed + _formationIndex * 0.37f) * _bobAmplitude;
            float scale = Mathf.MoveTowards(transform.localScale.x, _spawnScale, Time.deltaTime * 7f);
            transform.localScale = Vector3.one * scale;
            _shootKickTimer = Mathf.Max(0f, _shootKickTimer - Time.deltaTime);
            float shootKick = _shootKickTimer > 0f ? Mathf.Sin((_shootKickTimer / 0.16f) * Mathf.PI) : 0f;

            if (_bodyRoot != null)
            {
                _bodyRoot.localPosition = new Vector3(0f, bob, -shootKick * 0.035f);
                _bodyRoot.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * _bobSpeed + _formationIndex) * 4f - shootKick * 4f, 0f, 0f);
            }

            if (_weaponRoot != null)
            {
                _weaponRoot.localRotation = Quaternion.Euler(-4f + Mathf.Sin(Time.time * 11f + _formationIndex) * 2f - shootKick * 13f, 0f, 0f);
            }
        }

        public void SetTargetLocalPosition(Vector3 localPosition, int formationIndex)
        {
            _targetLocalPosition = localPosition;
            _formationIndex = formationIndex;
        }

        public void Spawn()
        {
            _isDespawning = false;
            _deathTimer = 0f;
            _shootKickTimer = 0f;
            _spawnScale = 1f;
            transform.localScale = Vector3.one * 0.2f;
            if (_bodyRoot != null)
            {
                _bodyRoot.localPosition = Vector3.zero;
                _bodyRoot.localRotation = Quaternion.identity;
            }
            if (_weaponRoot != null)
            {
                _weaponRoot.localRotation = Quaternion.identity;
            }
            gameObject.SetActive(true);
        }

        public void PlayShootKick()
        {
            if (!_isDespawning)
            {
                _shootKickTimer = 0.16f;
            }
        }

        public void Despawn(bool animated = true)
        {
            if (!animated || !gameObject.activeInHierarchy)
            {
                Release();
                return;
            }

            if (_isDespawning)
            {
                return;
            }

            _isDespawning = true;
            _deathTimer = 0f;
            _spawnScale = 0f;
            _deathStartPosition = transform.position;
            float side = (_formationIndex % 2 == 0 ? 1f : -1f) * (0.35f + (_formationIndex % 5) * 0.04f);
            _deathVelocity = new Vector3(side, 1.35f + (_formationIndex % 3) * 0.1f, -0.35f);
            _deathSpin = 90f + (_formationIndex % 7) * 15f;
            transform.SetParent(null, true);
        }

        private void UpdateDeathAnimation()
        {
            float duration = Mathf.Max(0.05f, _deathDuration);
            _deathTimer = Mathf.Min(duration, _deathTimer + Time.deltaTime);
            float t = Mathf.Clamp01(_deathTimer / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2.2f);
            transform.position = _deathStartPosition + _deathVelocity * t + Vector3.down * (1.55f * t * t);
            transform.rotation = Quaternion.Euler(eased * _deathSpin, eased * _deathSpin * 0.4f, eased * _deathSpin * 0.7f);
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.08f, eased);

            if (_bodyRoot != null)
            {
                _bodyRoot.localPosition = Vector3.zero;
            }

            if (_deathTimer >= duration)
            {
                Release();
            }
        }

        private void Release()
        {
            _isDespawning = false;
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
