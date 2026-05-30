using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArmyRush
{
    public sealed class GameplayUI : MonoBehaviour
    {
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _coinText;
        [SerializeField] private Text _stateText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private GameObject _bossPanel;
        [SerializeField] private Slider _bossSlider;
        [SerializeField] private Text _bossText;
        [SerializeField] private GameObject _startPrompt;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private Text _victoryCoinsText;
        [SerializeField] private Button _victoryRewardedButton;
        [SerializeField] private Text _victoryStatusText;
        [SerializeField] private Button _victoryUpgradeButton;
        [SerializeField] private GameObject _defeatPanel;
        [SerializeField] private Text _defeatText;
        [SerializeField] private Button _defeatReviveButton;
        [SerializeField] private Text _defeatStatusText;
        [SerializeField] private Button _defeatUpgradeButton;
        [SerializeField] private Image _defeatFadeImage;
        [SerializeField] private PlayerController _player;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private float _victoryCoinCountDuration = 0.65f;
        [SerializeField] private float _defeatFadeDuration = 0.36f;

        private EconomyService _economy;
        private ProgressionService _progression;
        private Coroutine _victoryCoinRoutine;
        private Coroutine _defeatFadeRoutine;
        private static readonly Color DefeatFadeColor = new Color(0.24f, 0.02f, 0.04f, 0.54f);

        private void Start()
        {
            ServiceLocator.TryGet(out _economy);
            ServiceLocator.TryGet(out _progression);
            if (_economy != null)
            {
                _economy.CoinsChanged += OnCoinsChanged;
                OnCoinsChanged(_economy.Coins);
            }
            if (_levelText != null)
            {
                int level = _progression != null ? _progression.CurrentLevelIndex : 1;
                _levelText.text = $"Level {level}";
            }
            EnsureDefeatFadeImage();
            EnsureResultUpgradeButtons();
            EnsureResultAdPlaceholders();
            RegisterResultButtons();
            SetRunState(RunState.PreRun);
        }

        private void OnEnable()
        {
            BossController.BossSpawned += OnBossSpawned;
            BossController.BossDefeated += OnBossDefeated;
            BossController.BossHealthChanged += OnBossHealthChanged;
        }

        private void OnDestroy()
        {
            BossController.BossSpawned -= OnBossSpawned;
            BossController.BossDefeated -= OnBossDefeated;
            BossController.BossHealthChanged -= OnBossHealthChanged;

            if (_economy != null)
            {
                _economy.CoinsChanged -= OnCoinsChanged;
            }
            UnregisterResultButtons();
        }

        private void Update()
        {
            if (_progressSlider != null && _player != null && _levelManager != null && _levelManager.CurrentLevel != null)
            {
                _progressSlider.value = Mathf.Clamp01(_player.ProgressZ / Mathf.Max(1f, _levelManager.CurrentLevel.trackLength));
            }
        }

        public void Configure(PlayerController player, LevelManager levelManager)
        {
            _player = player;
            _levelManager = levelManager;
        }

        public void SetRunState(RunState state)
        {
            if (_startPrompt != null)
            {
                _startPrompt.SetActive(state == RunState.PreRun);
            }
            if (_stateText != null)
            {
                _stateText.text = state == RunState.PreRun ? "DRAG TO START" : string.Empty;
            }
            if (_victoryPanel != null && state != RunState.Victory)
            {
                _victoryPanel.SetActive(false);
                StopVictoryCoinCount();
            }
            if (_defeatPanel != null && state != RunState.Defeat)
            {
                _defeatPanel.SetActive(false);
            }
            if (state != RunState.Defeat)
            {
                HideDefeatFade();
            }
        }

        public void ShowVictory(int coinsEarned)
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
            }
            if (_victoryCoinsText != null)
            {
                StopVictoryCoinCount();
                _victoryCoinRoutine = StartCoroutine(CountVictoryCoins(Mathf.Max(0, coinsEarned)));
            }
            SetResultStatus(_victoryStatusText, string.Empty);
        }

        public void ShowDefeat(int coinsEarned)
        {
            if (_defeatPanel != null)
            {
                _defeatPanel.SetActive(true);
            }
            if (_defeatText != null)
            {
                _defeatText.text = coinsEarned > 0 ? $"TRY AGAIN\n+{coinsEarned} COINS" : "TRY AGAIN";
            }
            SetResultStatus(_defeatStatusText, string.Empty);
            StartDefeatFade();
        }

        public void NextLevel()
        {
            SceneManager.LoadScene("Game");
        }

        public void Retry()
        {
            SceneManager.LoadScene("Game");
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void OpenUpgrades()
        {
            BackToMenu();
        }

        public void ShowRewardedPlaceholder()
        {
            PlayResultPlaceholderFeedback();
            SetResultStatus(_victoryStatusText, "REWARDED ADS COMING SOON");
        }

        public void ShowRevivePlaceholder()
        {
            PlayResultPlaceholderFeedback();
            SetResultStatus(_defeatStatusText, "REVIVE COMING SOON");
        }

        private void OnCoinsChanged(int coins)
        {
            if (_coinText != null)
            {
                _coinText.text = coins.ToString();
            }
        }

        private void OnBossSpawned(BossController boss)
        {
            if (_bossPanel != null)
            {
                _bossPanel.SetActive(true);
            }
            if (_bossText != null)
            {
                _bossText.text = boss.DisplayName;
            }
            if (_bossSlider != null)
            {
                _bossSlider.value = 1f;
            }
        }

        private void OnBossDefeated(BossController boss)
        {
            if (_bossPanel != null)
            {
                _bossPanel.SetActive(false);
            }
        }

        private void OnBossHealthChanged(BossController boss, float normalized)
        {
            if (_bossPanel != null)
            {
                _bossPanel.SetActive(normalized > 0f);
            }
            if (_bossSlider != null)
            {
                _bossSlider.value = Mathf.Clamp01(normalized);
            }
            if (_bossText != null)
            {
                _bossText.text = boss.DisplayName;
            }
        }

        private IEnumerator CountVictoryCoins(int targetCoins)
        {
            float duration = Mathf.Max(0.05f, _victoryCoinCountDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                int displayed = Mathf.RoundToInt(Mathf.Lerp(0f, targetCoins, eased));
                _victoryCoinsText.text = $"+{displayed} COINS";
                yield return null;
            }

            _victoryCoinsText.text = $"+{targetCoins} COINS";
            _victoryCoinRoutine = null;
        }

        private void StopVictoryCoinCount()
        {
            if (_victoryCoinRoutine == null)
            {
                return;
            }

            StopCoroutine(_victoryCoinRoutine);
            _victoryCoinRoutine = null;
        }

        private void EnsureResultUpgradeButtons()
        {
            RepositionResultActionButtons(_victoryPanel);
            RepositionResultActionButtons(_defeatPanel);
            _victoryUpgradeButton = EnsureResultUpgradeButton(_victoryPanel, _victoryUpgradeButton);
            _defeatUpgradeButton = EnsureResultUpgradeButton(_defeatPanel, _defeatUpgradeButton);
        }

        private void EnsureResultAdPlaceholders()
        {
            _victoryRewardedButton = EnsureResultPlaceholderButton(_victoryPanel, _victoryRewardedButton, "RewardedButton", "2X REWARD");
            _defeatReviveButton = EnsureResultPlaceholderButton(_defeatPanel, _defeatReviveButton, "ReviveButton", "REVIVE");
            _victoryStatusText = EnsureResultStatusText(_victoryPanel, _victoryStatusText);
            _defeatStatusText = EnsureResultStatusText(_defeatPanel, _defeatStatusText);
        }

        private void RepositionResultActionButtons(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            SetResultChildRect(panel.transform.Find("ActionButton"), new Vector2(0.32f, 0.16f), new Vector2(286f, 78f));
            SetResultChildRect(panel.transform.Find("UpgradeButton"), new Vector2(0.68f, 0.16f), new Vector2(286f, 78f));
        }

        private Button EnsureResultUpgradeButton(GameObject panel, Button currentButton)
        {
            if (currentButton != null || panel == null)
            {
                return currentButton;
            }

            Transform existing = panel.transform.Find("UpgradeButton");
            if (existing != null && existing.TryGetComponent(out Button existingButton))
            {
                return existingButton;
            }

            SetResultChildRect(panel.transform.Find("ActionButton"), new Vector2(0.32f, 0.16f), new Vector2(286f, 78f));

            return CreateResultButton("UpgradeButton", panel.transform, "UPGRADES", new Vector2(0.68f, 0.16f), new Vector2(286f, 78f), new Color(0.08f, 0.28f, 0.95f));
        }

        private Button EnsureResultPlaceholderButton(GameObject panel, Button currentButton, string name, string text)
        {
            if (currentButton != null || panel == null)
            {
                return currentButton;
            }

            Transform existing = panel.transform.Find(name);
            if (existing != null && existing.TryGetComponent(out Button existingButton))
            {
                SetResultChildRect(existing, new Vector2(0.5f, 0.36f), new Vector2(620f, 70f));
                return existingButton;
            }

            return CreateResultButton(name, panel.transform, text, new Vector2(0.5f, 0.36f), new Vector2(620f, 70f), new Color(0.05f, 0.13f, 0.24f));
        }

        private Text EnsureResultStatusText(GameObject panel, Text currentText)
        {
            if (currentText != null || panel == null)
            {
                return currentText;
            }

            Transform existing = panel.transform.Find("StatusText");
            if (existing != null && existing.TryGetComponent(out Text existingText))
            {
                SetResultChildRect(existing, new Vector2(0.5f, 0.27f), new Vector2(620f, 44f));
                return existingText;
            }

            Text status = CreateResultText("StatusText", panel.transform, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.27f), new Vector2(620f, 44f));
            status.color = new Color(1f, 0.78f, 0.12f);
            return status;
        }

        private void RegisterResultButtons()
        {
            if (_victoryUpgradeButton != null)
            {
                _victoryUpgradeButton.onClick.AddListener(OpenUpgrades);
            }
            if (_defeatUpgradeButton != null)
            {
                _defeatUpgradeButton.onClick.AddListener(OpenUpgrades);
            }
            if (_victoryRewardedButton != null)
            {
                _victoryRewardedButton.onClick.AddListener(ShowRewardedPlaceholder);
            }
            if (_defeatReviveButton != null)
            {
                _defeatReviveButton.onClick.AddListener(ShowRevivePlaceholder);
            }
        }

        private void UnregisterResultButtons()
        {
            if (_victoryUpgradeButton != null)
            {
                _victoryUpgradeButton.onClick.RemoveListener(OpenUpgrades);
            }
            if (_defeatUpgradeButton != null)
            {
                _defeatUpgradeButton.onClick.RemoveListener(OpenUpgrades);
            }
            if (_victoryRewardedButton != null)
            {
                _victoryRewardedButton.onClick.RemoveListener(ShowRewardedPlaceholder);
            }
            if (_defeatReviveButton != null)
            {
                _defeatReviveButton.onClick.RemoveListener(ShowRevivePlaceholder);
            }
        }

        private void EnsureDefeatFadeImage()
        {
            if (_defeatFadeImage != null)
            {
                _defeatFadeImage.raycastTarget = false;
                SetDefeatFadeAlpha(0f);
                _defeatFadeImage.gameObject.SetActive(false);
                return;
            }

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            GameObject fade = new GameObject("DefeatFadeOverlay", typeof(RectTransform), typeof(Image));
            RectTransform rect = fade.GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling();

            _defeatFadeImage = fade.GetComponent<Image>();
            _defeatFadeImage.raycastTarget = false;
            SetDefeatFadeAlpha(0f);
            fade.SetActive(false);
        }

        private void StartDefeatFade()
        {
            EnsureDefeatFadeImage();
            if (_defeatFadeImage == null)
            {
                return;
            }

            if (_defeatFadeRoutine != null)
            {
                StopCoroutine(_defeatFadeRoutine);
            }
            _defeatFadeImage.gameObject.SetActive(true);
            _defeatFadeRoutine = StartCoroutine(FadeDefeatOverlay());
        }

        private IEnumerator FadeDefeatOverlay()
        {
            float duration = Mathf.Max(0.05f, _defeatFadeDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                SetDefeatFadeAlpha(eased);
                yield return null;
            }

            SetDefeatFadeAlpha(1f);
            _defeatFadeRoutine = null;
        }

        private void HideDefeatFade()
        {
            if (_defeatFadeRoutine != null)
            {
                StopCoroutine(_defeatFadeRoutine);
                _defeatFadeRoutine = null;
            }

            if (_defeatFadeImage == null)
            {
                return;
            }

            SetDefeatFadeAlpha(0f);
            _defeatFadeImage.gameObject.SetActive(false);
        }

        private void SetDefeatFadeAlpha(float normalizedAlpha)
        {
            if (_defeatFadeImage == null)
            {
                return;
            }

            Color color = DefeatFadeColor;
            color.a *= Mathf.Clamp01(normalizedAlpha);
            _defeatFadeImage.color = color;
        }

        private static void SetResultStatus(Text statusText, string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private static void SetResultChildRect(Transform target, Vector2 anchorPosition, Vector2 size)
        {
            if (target == null || !target.TryGetComponent(out RectTransform rect))
            {
                return;
            }

            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void PlayResultPlaceholderFeedback()
        {
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.Button);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Light);
            }
        }

        private static Button CreateResultButton(string name, Transform parent, string text, Vector2 anchorPosition, Vector2 size, Color color)
        {
            GameObject root = new GameObject(name);
            RectTransform rect = root.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Image image = root.AddComponent<Image>();
            image.color = color;
            Button button = root.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.08f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.12f);
            colors.disabledColor = new Color(0.35f, 0.38f, 0.44f, 0.75f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            button.targetGraphic = image;
            root.AddComponent<SimpleButtonAnimator>();

            Text label = CreateResultButtonText(root.transform, text, size);
            label.color = Color.white;
            return button;
        }

        private static Text CreateResultText(string name, Transform parent, string text, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 anchorPosition, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Text label = obj.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                label.font = font;
            }

            return label;
        }

        private static Text CreateResultButtonText(Transform parent, string text, Vector2 size)
        {
            GameObject obj = new GameObject("Text");
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size * 0.9f;
            rect.anchoredPosition = Vector2.zero;

            Text label = obj.AddComponent<Text>();
            label.text = text;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                label.font = font;
            }
            label.fontSize = 42;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 18;
            label.resizeTextMaxSize = 42;
            return label;
        }
    }
}
