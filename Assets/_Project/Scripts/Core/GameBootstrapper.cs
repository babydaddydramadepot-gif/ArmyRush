using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArmyRush
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private UpgradeDefinition[] _upgradeDefinitions;
        [SerializeField] private bool _loadMainMenuOnStart = true;

        private static GameBootstrapper _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            ConfigureRuntimePerformance();
            EnsureServices(_upgradeDefinitions);
        }

        private void Start()
        {
            if (_loadMainMenuOnStart && SceneManager.GetActiveScene().name != _mainMenuSceneName)
            {
                SceneManager.LoadScene(_mainMenuSceneName);
            }
        }

        public static void EnsureServices(IEnumerable<UpgradeDefinition> upgradeDefinitions)
        {
            if (!ServiceLocator.Has<SaveService>())
            {
                SaveService saveService = new SaveService();
                saveService.Load();
                ServiceLocator.Register(saveService);
            }

            SaveService save = ServiceLocator.Get<SaveService>();

            if (!ServiceLocator.Has<EconomyService>())
            {
                ServiceLocator.Register(new EconomyService(save));
            }

            if (!ServiceLocator.Has<ProgressionService>())
            {
                ServiceLocator.Register(new ProgressionService(save));
            }

            if (!ServiceLocator.Has<UpgradeService>())
            {
                ServiceLocator.Register(new UpgradeService(save, ServiceLocator.Get<EconomyService>(), upgradeDefinitions));
            }

            if (!ServiceLocator.Has<AudioService>())
            {
                ServiceLocator.Register(new AudioService(save));
            }

            if (!ServiceLocator.Has<HapticsService>())
            {
                ServiceLocator.Register(new HapticsService(save));
            }
        }

        private static void ConfigureRuntimePerformance()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = true;
        }
    }
}
