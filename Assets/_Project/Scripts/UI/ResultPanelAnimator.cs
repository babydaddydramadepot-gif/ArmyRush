using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ResultPanelAnimator : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.24f;
        [SerializeField] private float _startScale = 0.94f;

        private CanvasGroup _canvasGroup;
        private Vector3 _baseScale;
        private float _elapsed;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _elapsed = 0f;
            _baseScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
            transform.localScale = _baseScale * _startScale;
            _canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            float duration = Mathf.Max(0.01f, _duration);
            _elapsed = Mathf.Min(duration, _elapsed + Time.unscaledDeltaTime);
            float t = Mathf.Clamp01(_elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = eased;
            }
            transform.localScale = Vector3.LerpUnclamped(_baseScale * _startScale, _baseScale, eased);
        }
    }
}
