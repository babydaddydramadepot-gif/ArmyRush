using UnityEngine;

namespace ArmyRush
{
    public sealed class PooledParticleVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] _systems;
        [SerializeField] private float _lifetime = 1f;

        private PooledObject _pooledObject;
        private float _remaining;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObject>();
            if (_systems == null || _systems.Length == 0)
            {
                _systems = GetComponentsInChildren<ParticleSystem>(true);
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

        public void Play()
        {
            _remaining = Mathf.Max(0.05f, _lifetime);
            if (_systems == null)
            {
                return;
            }

            for (int i = 0; i < _systems.Length; i++)
            {
                if (_systems[i] == null)
                {
                    continue;
                }

                _systems[i].Clear(true);
                _systems[i].Play(true);
            }
        }

        public void Configure(ParticleSystem[] systems, float lifetime)
        {
            _systems = systems;
            _lifetime = Mathf.Max(0.05f, lifetime);
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
