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
        [SerializeField] private GameObject _defeatPanel;
        [SerializeField] private Text _defeatText;
        [SerializeField] private PlayerController _player;
        [SerializeField] private LevelManager _levelManager;

        private EconomyService _economy;
        private ProgressionService _progression;

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
            }
            if (_defeatPanel != null && state != RunState.Defeat)
            {
                _defeatPanel.SetActive(false);
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
                _victoryCoinsText.text = $"+{coinsEarned} COINS";
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
    }
}
