using UnityEngine;
using UnityEngine.UI;

namespace ArmyRush
{
    public sealed class UpgradeButtonView : MonoBehaviour
    {
        [SerializeField] private UpgradeType _upgradeType;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _costText;
        [SerializeField] private Button _button;

        private UpgradeService _service;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            if (_button != null)
            {
                _button.onClick.AddListener(Purchase);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(Purchase);
            }
        }

        public void Configure(UpgradeType type)
        {
            _upgradeType = type;
        }

        public void Bind(UpgradeService service)
        {
            _service = service;
            Refresh();
        }

        private void Purchase()
        {
            if (_service != null && _service.Purchase(_upgradeType))
            {
                if (ServiceLocator.TryGet(out AudioService audio))
                {
                    audio.Play(AudioCue.Upgrade);
                }
                if (ServiceLocator.TryGet(out HapticsService haptics))
                {
                    haptics.Play(HapticCue.Light);
                }
                Refresh();
            }
        }

        private void Refresh()
        {
            if (_service == null || !_service.TryGetDefinition(_upgradeType, out UpgradeDefinition definition))
            {
                return;
            }

            int level = _service.GetLevel(_upgradeType);
            int cost = definition.GetCost(level);
            bool canPurchase = _service.CanPurchase(_upgradeType);

            if (_titleText != null)
            {
                _titleText.text = definition.displayName.ToUpperInvariant();
            }
            if (_levelText != null)
            {
                _levelText.text = $"Lv. {level}";
            }
            if (_costText != null)
            {
                _costText.text = definition.IsMaxed(level) ? "MAX" : cost.ToString();
            }
            if (_button != null)
            {
                _button.interactable = canPurchase;
            }
        }
    }
}
