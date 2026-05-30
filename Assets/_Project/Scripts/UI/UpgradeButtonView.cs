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
        [SerializeField] private Image _backgroundImage;

        private static readonly Color AffordableCostColor = new Color(1f, 0.83f, 0.2f);
        private static readonly Color BlockedCostColor = new Color(1f, 0.45f, 0.32f);
        private static readonly Color MaxedCostColor = new Color(0.35f, 1f, 0.62f);
        private static readonly Color MutedTextColor = new Color(0.72f, 0.8f, 0.92f);
        private static readonly Color PurchaseFlashColor = new Color(0.35f, 1f, 0.62f);
        private static readonly Color BlockedBackgroundColor = new Color(0.13f, 0.18f, 0.26f, 1f);
        private static readonly Color MaxedBackgroundColor = new Color(0.05f, 0.38f, 0.22f, 1f);
        private const float PurchaseFeedbackDuration = 0.42f;

        private UpgradeService _service;
        private float _purchaseFeedbackTimer;
        private Color _baseBackgroundColor = new Color(0.08f, 0.28f, 0.95f, 1f);

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            if (_backgroundImage == null)
            {
                _backgroundImage = GetComponent<Image>();
            }
            if (_backgroundImage != null)
            {
                _baseBackgroundColor = _backgroundImage.color;
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

        private void Update()
        {
            if (_purchaseFeedbackTimer <= 0f)
            {
                return;
            }

            _purchaseFeedbackTimer -= Time.unscaledDeltaTime;
            if (_purchaseFeedbackTimer <= 0f)
            {
                Refresh();
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * 10f, 1f);
            Color flash = Color.Lerp(PurchaseFlashColor, Color.white, pulse * 0.35f);
            if (_costText != null)
            {
                _costText.text = "BOUGHT";
                _costText.color = flash;
            }
            if (_levelText != null)
            {
                _levelText.color = flash;
            }
            if (_backgroundImage != null)
            {
                _backgroundImage.color = Color.Lerp(_baseBackgroundColor, PurchaseFlashColor, 0.45f + pulse * 0.4f);
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
                _purchaseFeedbackTimer = PurchaseFeedbackDuration;
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
            bool isMaxed = definition.IsMaxed(level);
            bool canPurchase = _service.CanPurchase(_upgradeType);
            bool showingPurchaseFeedback = _purchaseFeedbackTimer > 0f;
            Color primaryTextColor = canPurchase || isMaxed ? Color.white : MutedTextColor;
            Color backgroundColor = showingPurchaseFeedback ? PurchaseFlashColor : isMaxed ? MaxedBackgroundColor : canPurchase ? _baseBackgroundColor : BlockedBackgroundColor;

            if (_titleText != null)
            {
                _titleText.text = definition.displayName.ToUpperInvariant();
                _titleText.color = primaryTextColor;
            }
            if (_levelText != null)
            {
                _levelText.text = $"Lv. {level}";
                _levelText.color = showingPurchaseFeedback ? PurchaseFlashColor : primaryTextColor;
            }
            if (_costText != null)
            {
                _costText.text = showingPurchaseFeedback ? "BOUGHT" : isMaxed ? "MAX" : cost.ToString();
                _costText.color = showingPurchaseFeedback ? PurchaseFlashColor : isMaxed ? MaxedCostColor : canPurchase ? AffordableCostColor : BlockedCostColor;
            }
            if (_button != null)
            {
                _button.interactable = canPurchase;
            }
            if (_backgroundImage != null)
            {
                _backgroundImage.color = backgroundColor;
            }
        }
    }
}
