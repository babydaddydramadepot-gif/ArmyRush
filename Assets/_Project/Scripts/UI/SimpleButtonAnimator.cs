using UnityEngine;
using UnityEngine.EventSystems;

namespace ArmyRush
{
    public sealed class SimpleButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Vector3 _baseScale;
        private float _targetScale = 1f;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _baseScale * _targetScale, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _targetScale = 0.94f;
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.Button);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _targetScale = 1.05f;
            Invoke(nameof(ReturnToBase), 0.08f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetScale = 1f;
        }

        private void ReturnToBase()
        {
            _targetScale = 1f;
        }
    }
}
