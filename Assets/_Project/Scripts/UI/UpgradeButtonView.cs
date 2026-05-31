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
        [SerializeField] private Image _glowImage;

        private static readonly Color AffordableCostColor = new Color(1f, 0.83f, 0.2f);
        private static readonly Color BlockedCostColor = new Color(1f, 0.45f, 0.32f);
        private static readonly Color MaxedCostColor = new Color(0.35f, 1f, 0.62f);
        private static readonly Color RecommendedCostColor = new Color(1f, 0.92f, 0.35f);
        private static readonly Color RecommendedGlowColor = new Color(1f, 0.74f, 0.18f);
        private static readonly Color MutedTextColor = new Color(0.72f, 0.8f, 0.92f);
        private static readonly Color PurchaseFlashColor = new Color(0.35f, 1f, 0.62f);
        private static readonly Color BlockedBackgroundColor = new Color(0.13f, 0.18f, 0.26f, 1f);
        private static readonly Color RecommendedBackgroundColor = new Color(0.08f, 0.36f, 0.95f, 1f);
        private static readonly Color MaxedBackgroundColor = new Color(0.05f, 0.38f, 0.22f, 1f);
        private const float PurchaseFeedbackDuration = 0.42f;

        private UpgradeService _service;
        private float _purchaseFeedbackTimer;
        private bool _isRecommended;
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
            EnsureGlowImage();
            ConfigureTextFit(_titleText, 13, 21);
            ConfigureTextFit(_levelText, 10, 18);
            ConfigureTextFit(_costText, 12, 22);
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
                if (_isRecommended)
                {
                    AnimateRecommendedGlow();
                }
                return;
            }

            _purchaseFeedbackTimer -= Time.unscaledDeltaTime;
            if (_purchaseFeedbackTimer <= 0f)
            {
                Refresh();
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * 10f, 1f);
            float elapsed = 1f - Mathf.Clamp01(_purchaseFeedbackTimer / PurchaseFeedbackDuration);
            float glowWeight = Mathf.Sin(elapsed * Mathf.PI);
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
            if (_glowImage != null)
            {
                _glowImage.gameObject.SetActive(true);
                _glowImage.color = new Color(PurchaseFlashColor.r, PurchaseFlashColor.g, PurchaseFlashColor.b, 0.38f * glowWeight);
                _glowImage.transform.localScale = Vector3.one * (1f + glowWeight * 0.12f);
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
                _isRecommended = false;
                return;
            }

            int level = _service.GetLevel(_upgradeType);
            int cost = definition.GetCost(level);
            bool isUnlocked = _service.IsUnlocked(_upgradeType);
            bool isMaxed = definition.IsMaxed(level);
            bool canPurchase = _service.CanPurchase(_upgradeType);
            bool showingPurchaseFeedback = _purchaseFeedbackTimer > 0f;
            bool recommended = canPurchase && _service.IsRecommendedPurchase(_upgradeType);
            _isRecommended = recommended && !showingPurchaseFeedback;
            Color primaryTextColor = isUnlocked && (canPurchase || isMaxed) ? Color.white : MutedTextColor;
            Color backgroundColor = showingPurchaseFeedback
                ? PurchaseFlashColor
                : isMaxed
                    ? MaxedBackgroundColor
                    : recommended
                        ? RecommendedBackgroundColor
                        : canPurchase
                            ? _baseBackgroundColor
                            : BlockedBackgroundColor;

            if (_titleText != null)
            {
                _titleText.text = definition.displayName.ToUpperInvariant();
                _titleText.color = primaryTextColor;
            }
            if (_levelText != null)
            {
                _levelText.text = BuildLevelPreview(definition, level, isUnlocked, isMaxed);
                _levelText.color = showingPurchaseFeedback ? PurchaseFlashColor : primaryTextColor;
            }
            if (_costText != null)
            {
                _costText.text = showingPurchaseFeedback ? "BOUGHT" : !isUnlocked ? $"LV {definition.unlockLevel}" : isMaxed ? "MAX" : cost.ToString();
                _costText.color = showingPurchaseFeedback ? PurchaseFlashColor : isMaxed ? MaxedCostColor : recommended ? RecommendedCostColor : canPurchase ? AffordableCostColor : BlockedCostColor;
            }
            if (_button != null)
            {
                _button.interactable = canPurchase;
            }
            if (_backgroundImage != null)
            {
                _backgroundImage.color = backgroundColor;
            }
            if (_glowImage != null && !showingPurchaseFeedback)
            {
                if (_isRecommended)
                {
                    AnimateRecommendedGlow();
                }
                else
                {
                    _glowImage.color = Color.clear;
                    _glowImage.gameObject.SetActive(false);
                    _glowImage.transform.localScale = Vector3.one;
                }
            }
        }

        private string BuildLevelPreview(UpgradeDefinition definition, int level, bool isUnlocked, bool isMaxed)
        {
            if (!isUnlocked)
            {
                return $"Unlock Lv. {definition.unlockLevel}";
            }
            if (isMaxed)
            {
                return $"Lv. {level}";
            }

            return $"Lv. {level} {FormatValue(definition.GetValue(level))}>{FormatValue(definition.GetValue(level + 1))}";
        }

        private string FormatValue(float value)
        {
            switch (_upgradeType)
            {
                case UpgradeType.StartingTroops:
                case UpgradeType.Damage:
                    return Mathf.RoundToInt(value).ToString();
                case UpgradeType.CriticalChance:
                    return Mathf.RoundToInt(value * 100f) + "%";
                case UpgradeType.CoinReward:
                    return value.ToString("0.00") + "x";
                case UpgradeType.FireRate:
                case UpgradeType.BossDamage:
                case UpgradeType.ObstacleDamage:
                case UpgradeType.CriticalDamage:
                    return value.ToString("0.0") + "x";
                default:
                    return value.ToString("0.0");
            }
        }

        private void AnimateRecommendedGlow()
        {
            if (_glowImage == null)
            {
                return;
            }

            float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 5.5f) * 0.5f;
            _glowImage.gameObject.SetActive(true);
            _glowImage.color = new Color(RecommendedGlowColor.r, RecommendedGlowColor.g, RecommendedGlowColor.b, 0.16f + pulse * 0.18f);
            _glowImage.transform.localScale = Vector3.one * (1.02f + pulse * 0.08f);
        }

        private void EnsureGlowImage()
        {
            if (_glowImage == null)
            {
                Transform existing = transform.Find("UpgradeGlow");
                if (existing != null)
                {
                    existing.TryGetComponent(out _glowImage);
                }
            }

            if (_glowImage == null)
            {
                GameObject glow = new GameObject("UpgradeGlow");
                glow.transform.SetParent(transform, false);
                glow.transform.SetAsFirstSibling();
                RectTransform rect = glow.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(-8f, -8f);
                rect.offsetMax = new Vector2(8f, 8f);
                _glowImage = glow.AddComponent<Image>();
            }

            _glowImage.raycastTarget = false;
            _glowImage.color = Color.clear;
            _glowImage.gameObject.SetActive(false);
        }

        private static void ConfigureTextFit(Text text, int minSize, int maxSize)
        {
            if (text == null)
            {
                return;
            }

            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = maxSize;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }
    }
}
