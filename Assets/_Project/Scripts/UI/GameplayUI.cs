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
        [SerializeField] private Button _victoryUpgradeButton;
        [SerializeField] private GameObject _defeatPanel;
        [SerializeField] private Text _defeatText;
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
            _victoryUpgradeButton = EnsureResultUpgradeButton(_victoryPanel, _victoryUpgradeButton);
            _defeatUpgradeButton = EnsureResultUpgradeButton(_defeatPanel, _defeatUpgradeButton);
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

            Transform action = panel.transform.Find("ActionButton");
            if (action != null && action.TryGetComponent(out RectTransform actionRect))
            {
                actionRect.anchorMin = new Vector2(0.32f, 0.22f);
                actionRect.anchorMax = new Vector2(0.32f, 0.22f);
                actionRect.sizeDelta = new Vector2(300f, 96f);
                actionRect.anchoredPosition = Vector2.zero;
            }

            return CreateResultButton("UpgradeButton", panel.transform, "UPGRADES", new Vector2(0.68f, 0.22f), new Vector2(300f, 96f), new Color(0.08f, 0.28f, 0.95f));
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
