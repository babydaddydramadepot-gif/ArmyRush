using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArmyRush
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private UpgradeDefinition[] _upgradeDefinitions;
        [SerializeField] private Text _coinsText;
        [SerializeField] private Text _levelText;
        [SerializeField] private UpgradeButtonView[] _upgradeButtons;

        private EconomyService _economy;
        private ProgressionService _progression;
        private UpgradeService _upgrades;

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
            if (_levelText != null)
            {
                _levelText.text = $"LEVEL {_progression?.CurrentLevelIndex ?? 1}";
            }

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
    }
}
