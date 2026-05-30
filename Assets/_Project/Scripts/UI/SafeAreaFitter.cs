using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea || _lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastSafeArea = Screen.safeArea;
            float width = Mathf.Max(1f, _lastScreenWidth);
            float height = Mathf.Max(1f, _lastScreenHeight);
            Vector2 anchorMin = _lastSafeArea.position;
            Vector2 anchorMax = _lastSafeArea.position + _lastSafeArea.size;
            anchorMin.x = Mathf.Clamp01(anchorMin.x / width);
            anchorMin.y = Mathf.Clamp01(anchorMin.y / height);
            anchorMax.x = Mathf.Clamp01(anchorMax.x / width);
            anchorMax.y = Mathf.Clamp01(anchorMax.y / height);
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
