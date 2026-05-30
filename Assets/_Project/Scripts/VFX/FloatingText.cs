using UnityEngine;

namespace ArmyRush
{
    public sealed class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMesh _label;
        [SerializeField] private float _lifetime = 0.85f;
        [SerializeField] private float _riseSpeed = 1.35f;
        [SerializeField] private float _scalePunch = 1.18f;

        private PooledObject _pooledObject;
        private float _remaining;
        private Color _color;
        private Vector3 _startScale;
        private Camera _camera;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObject>();
            if (_label == null)
            {
                _label = GetComponentInChildren<TextMesh>();
            }
            _startScale = transform.localScale;
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);

            float normalized = Mathf.Clamp01(_remaining / Mathf.Max(0.01f, _lifetime));
            float alpha = Mathf.SmoothStep(0f, 1f, normalized);
            float punch = 1f + Mathf.Sin((1f - normalized) * Mathf.PI) * (_scalePunch - 1f);
            transform.localScale = _startScale * punch;

            if (_label != null)
            {
                Color color = _color;
                color.a = alpha;
                _label.color = color;
                if (_camera == null)
                {
                    _camera = Billboard.SharedCamera;
                }
                if (_camera != null)
                {
                    _label.transform.rotation = Quaternion.LookRotation(_label.transform.position - _camera.transform.position, Vector3.up);
                }
            }

            if (_remaining <= 0f)
            {
                Release();
            }
        }

        public void Play(string text, Color color)
        {
            if (_label != null)
            {
                _label.text = text;
                _label.color = color;
            }
            _color = color;
            _remaining = _lifetime;
            transform.localScale = _startScale;
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
