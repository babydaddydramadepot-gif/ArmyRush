using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArmyRush
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private UpgradeDefinition[] _upgradeDefinitions;
        [SerializeField] private LevelData[] _levelDefinitions;
        [SerializeField] private Text _coinsText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _milestoneText;
        [SerializeField] private UpgradeButtonView[] _upgradeButtons;

        private EconomyService _economy;
        private ProgressionService _progression;
        private UpgradeService _upgrades;
        private const int BossLevelInterval = 5;
        private static readonly Color MilestoneDefaultColor = new Color(1f, 0.78f, 0.12f);
        private static readonly Color MilestoneActiveColor = new Color(1f, 0.9f, 0.32f);

        private void Awake()
        {
            GameBootstrapper.EnsureServices(_upgradeDefinitions);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _economy);
            ServiceLocator.TryGet(out _progression);
            ServiceLocator.TryGet(out _upgrades);

            if (_economy != null)
            {
                _economy.CoinsChanged += OnCoinsChanged;
                OnCoinsChanged(_economy.Coins);
            }
            if (_upgrades != null)
            {
                _upgrades.UpgradePurchased += OnUpgradePurchased;
            }

            EnsureMilestoneText();
            Refresh();
        }

        private void OnDestroy()
        {
            if (_economy != null)
            {
                _economy.CoinsChanged -= OnCoinsChanged;
            }
            if (_upgrades != null)
            {
                _upgrades.UpgradePurchased -= OnUpgradePurchased;
            }
        }

        public void Play()
        {
            SceneManager.LoadScene("Game");
        }

        public void Refresh()
        {
            int currentLevel = _progression?.CurrentLevelIndex ?? 1;
            if (_levelText != null)
            {
                _levelText.text = $"LEVEL {currentLevel}";
            }
            RefreshMilestoneText(currentLevel);

            if (_upgradeButtons == null || _upgrades == null)
            {
                return;
            }

            for (int i = 0; i < _upgradeButtons.Length; i++)
            {
                if (_upgradeButtons[i] != null)
                {
                    _upgradeButtons[i].Bind(_upgrades);
                }
            }
        }

        private void OnCoinsChanged(int coins)
        {
            if (_coinsText != null)
            {
                _coinsText.text = coins.ToString();
            }
            Refresh();
        }

        private void OnUpgradePurchased(UpgradeType type, int level)
        {
            Refresh();
        }

        private void EnsureMilestoneText()
        {
            if (_milestoneText != null)
            {
                ConfigureMilestoneText(_milestoneText);
                return;
            }

            Transform safeArea = transform.Find("SafeArea");
            Transform parent = safeArea != null ? safeArea : transform;
            Transform existing = parent.Find("BossMilestoneText");
            if (existing != null && existing.TryGetComponent(out Text existingText))
            {
                _milestoneText = existingText;
                ConfigureMilestoneText(_milestoneText);
                return;
            }

            GameObject labelObject = new GameObject("BossMilestoneText", typeof(RectTransform), typeof(Text));
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.722f);
            rect.anchorMax = new Vector2(0.5f, 0.722f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(560f, 54f);
            rect.anchoredPosition = Vector2.zero;

            _milestoneText = labelObject.GetComponent<Text>();
            ConfigureMilestoneText(_milestoneText);
        }

        private void ConfigureMilestoneText(Text label)
        {
            if (label == null)
            {
                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                label.font = font;
            }
            label.fontSize = 28;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = 28;
            label.raycastTarget = false;

            if (label.TryGetComponent(out RectTransform rect))
            {
                rect.anchorMin = new Vector2(0.5f, 0.722f);
                rect.anchorMax = new Vector2(0.5f, 0.722f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(560f, 54f);
                rect.anchoredPosition = Vector2.zero;
            }
        }

        private void RefreshMilestoneText(int currentLevel)
        {
            EnsureMilestoneText();
            if (_milestoneText == null)
            {
                return;
            }

            int nextBossLevel = GetNextBossLevel(Mathf.Max(1, currentLevel));
            int levelsUntilBoss = Mathf.Max(0, nextBossLevel - Mathf.Max(1, currentLevel));
            if (levelsUntilBoss == 0)
            {
                _milestoneText.text = "BOSS LEVEL";
                _milestoneText.color = MilestoneActiveColor;
            }
            else if (levelsUntilBoss == 1)
            {
                _milestoneText.text = "BOSS NEXT";
                _milestoneText.color = MilestoneActiveColor;
            }
            else
            {
                _milestoneText.text = $"BOSS IN {levelsUntilBoss} LEVELS";
                _milestoneText.color = MilestoneDefaultColor;
            }
        }

        private int GetNextBossLevel(int currentLevel)
        {
            int nextBossLevel = int.MaxValue;
            if (_levelDefinitions != null)
            {
                for (int i = 0; i < _levelDefinitions.Length; i++)
                {
                    LevelData level = _levelDefinitions[i];
                    if (level != null && level.hasBoss && level.levelIndex >= currentLevel && level.levelIndex < nextBossLevel)
                    {
                        nextBossLevel = level.levelIndex;
                    }
                }
            }

            if (nextBossLevel != int.MaxValue)
            {
                return nextBossLevel;
            }

            int remainder = currentLevel % BossLevelInterval;
            return remainder == 0 ? currentLevel : currentLevel + (BossLevelInterval - remainder);
        }
    }
}
