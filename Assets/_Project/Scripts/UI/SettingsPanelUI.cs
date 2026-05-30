using UnityEngine;
using UnityEngine.UI;

namespace ArmyRush
{
    public sealed class SettingsPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Toggle _hapticsToggle;
        [SerializeField] private Button _closeButton;

        private SaveService _saveService;
        private bool _refreshing;

        private void Start()
        {
            ServiceLocator.TryGet(out _saveService);

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

        private void SetSfxVolume(float value)
        {
            if (_refreshing || _saveService == null)
            {
                return;
            }

            _saveService.Data.sfxVolume = Mathf.Clamp01(value);
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
    }
}
