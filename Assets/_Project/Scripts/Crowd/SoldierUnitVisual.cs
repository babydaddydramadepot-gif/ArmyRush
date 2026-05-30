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

        private Vector3 _targetLocalPosition;
        private int _formationIndex;
        private PooledObject _pooledObject;
        private float _spawnScale;

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
            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetLocalPosition, 1f - Mathf.Exp(-_moveLerp * Time.deltaTime));
            float bob = Mathf.Sin(Time.time * _bobSpeed + _formationIndex * 0.37f) * _bobAmplitude;
            float scale = Mathf.MoveTowards(transform.localScale.x, _spawnScale, Time.deltaTime * 7f);
            transform.localScale = Vector3.one * scale;

            if (_bodyRoot != null)
            {
                _bodyRoot.localPosition = new Vector3(0f, bob, 0f);
                _bodyRoot.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * _bobSpeed + _formationIndex) * 4f, 0f, 0f);
            }

            if (_weaponRoot != null)
            {
                _weaponRoot.localRotation = Quaternion.Euler(-4f + Mathf.Sin(Time.time * 11f + _formationIndex) * 2f, 0f, 0f);
            }
        }

        public void SetTargetLocalPosition(Vector3 localPosition, int formationIndex)
        {
            _targetLocalPosition = localPosition;
            _formationIndex = formationIndex;
        }

        public void Spawn()
        {
            _spawnScale = 1f;
            transform.localScale = Vector3.one * 0.2f;
            gameObject.SetActive(true);
        }

        public void Despawn()
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
