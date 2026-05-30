using UnityEngine;

namespace ArmyRush
{
    public sealed class VfxManager : MonoBehaviour
    {
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private GameObject _floatingTextPrefab;

        private static VfxManager _active;

        private void Awake()
        {
            _active = this;
            if (_poolManager != null && _floatingTextPrefab != null)
            {
                _poolManager.Prewarm(_floatingTextPrefab, 24);
            }
        }

        private void OnDestroy()
        {
            if (_active == this)
            {
                _active = null;
            }
        }

        public void Configure(PoolManager poolManager, GameObject floatingTextPrefab)
        {
            _poolManager = poolManager;
            _floatingTextPrefab = floatingTextPrefab;
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
    }
}
