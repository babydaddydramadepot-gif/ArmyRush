using UnityEngine;
using UnityEngine.UI;

namespace ArmyRush
{
    public sealed class SettingsPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Toggle _hapticsToggle;
        [SerializeField] private Button _closeButton;

        private SaveService _saveService;
        private bool _refreshing;

        private void Start()
        {
            ServiceLocator.TryGet(out _saveService);
            EnsureRuntimeMusicSlider();

            if (_musicSlider != null)
            {
                _musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            }
            if (_hapticsToggle != null)
            {
                _hapticsToggle.onValueChanged.AddListener(SetHapticsEnabled);
            }
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            Refresh();
            Close();
        }

        private void OnDestroy()
        {
            if (_musicSlider != null)
            {
                _musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            }
            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
            }
            if (_hapticsToggle != null)
            {
                _hapticsToggle.onValueChanged.RemoveListener(SetHapticsEnabled);
            }
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
            }
        }

        public void Open()
        {
            Refresh();
            if (_panel != null)
            {
                _panel.SetActive(true);
            }
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.Button);
            }
        }

        public void Close()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        private void SetMusicVolume(float value)
        {
            if (_refreshing || _saveService == null)
            {
                return;
            }

            float volume = Mathf.Clamp01(value);
            _saveService.Data.musicVolume = volume;
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.SetMusicVolume(volume);
            }
            _saveService.Save();
        }

        private void SetSfxVolume(float value)
        {
            if (_refreshing || _saveService == null)
            {
                return;
            }

            float volume = Mathf.Clamp01(value);
            _saveService.Data.sfxVolume = volume;
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.SetSfxVolume(volume);
            }
            _saveService.Save();
        }

        private void SetHapticsEnabled(bool enabled)
        {
            if (_refreshing || _saveService == null)
            {
                return;
            }

            _saveService.Data.hapticsEnabled = enabled;
            _saveService.Save();
            if (enabled && ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Light);
            }
        }

        private void Refresh()
        {
            _refreshing = true;
            if (_saveService != null)
            {
                if (_musicSlider != null)
                {
                    _musicSlider.value = Mathf.Clamp01(_saveService.Data.musicVolume);
                }
                if (_sfxSlider != null)
                {
                    _sfxSlider.value = Mathf.Clamp01(_saveService.Data.sfxVolume);
                }
                if (_hapticsToggle != null)
                {
                    _hapticsToggle.isOn = _saveService.Data.hapticsEnabled;
                }
            }
            _refreshing = false;
        }

        private void EnsureRuntimeMusicSlider()
        {
            if (_musicSlider != null || _panel == null)
            {
                return;
            }

            Transform existing = _panel.transform.Find("MusicSlider");
            if (existing != null && existing.TryGetComponent(out Slider existingSlider))
            {
                _musicSlider = existingSlider;
                return;
            }

            RepositionSettingsControls();
            CreateRuntimeText("MusicLabel", _panel.transform, "MUSIC", 34, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.24f, 0.68f), new Vector2(220f, 58f));
            _musicSlider = CreateRuntimeSlider("MusicSlider", _panel.transform, new Vector2(0.62f, 0.68f), new Vector2(390f, 34f));
        }

        private void RepositionSettingsControls()
        {
            SetAnchorY(_panel.transform.Find("SfxLabel"), 0.54f);
            if (_sfxSlider != null)
            {
                SetAnchorY(_sfxSlider.transform, 0.54f);
            }
            if (_hapticsToggle != null)
            {
                SetAnchorY(_hapticsToggle.transform, 0.39f);
            }
            if (_closeButton != null)
            {
                SetAnchorY(_closeButton.transform, 0.17f);
            }
        }

        private static void SetAnchorY(Transform target, float y)
        {
            if (target == null || !target.TryGetComponent(out RectTransform rect))
            {
                return;
            }

            rect.anchorMin = new Vector2(rect.anchorMin.x, y);
            rect.anchorMax = new Vector2(rect.anchorMax.x, y);
            rect.anchoredPosition = Vector2.zero;
        }

        private static Text CreateRuntimeText(string name, Transform parent, string text, int size, FontStyle style, TextAnchor anchor, Color color, Vector2 anchorPosition, Vector2 rectSize)
        {
            GameObject obj = new GameObject(name);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = rectSize;
            rect.anchoredPosition = Vector2.zero;

            Text label = obj.AddComponent<Text>();
            label.text = text;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                label.font = font;
            }
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = anchor;
            label.color = color;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Mathf.Max(12, Mathf.RoundToInt(size * 0.55f));
            label.resizeTextMaxSize = size;
            return label;
        }

        private static Slider CreateRuntimeSlider(string name, Transform parent, Vector2 anchorPosition, Vector2 size)
        {
            GameObject root = CreateRuntimePanel(name, parent, anchorPosition, size, new Color(0.02f, 0.06f, 0.11f, 0.92f));
            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.interactable = true;

            GameObject fill = CreateRuntimePanel("Fill", root.transform, new Vector2(0f, 0.5f), size, new Color(0.1f, 0.86f, 0.36f, 1f));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            slider.fillRect = fillRect;

            GameObject handle = CreateRuntimePanel("Handle", root.transform, new Vector2(1f, 0.5f), new Vector2(52f, 52f), Color.white);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(1f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            return slider;
        }

        private static GameObject CreateRuntimePanel(string name, Transform parent, Vector2 anchorPosition, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            Image image = obj.AddComponent<Image>();
            image.color = color;
            return obj;
        }
    }
}
