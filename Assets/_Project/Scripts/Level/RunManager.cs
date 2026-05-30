using System;
using UnityEngine;

namespace ArmyRush
{
    public sealed class RunManager : MonoBehaviour
    {
        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private CrowdManager _crowd;
        [SerializeField] private GameplayUI _gameplayUI;

        private int _runCoins;
        private int _bonusCoins;
        private bool _levelCompleted;
        private EconomyService _economy;
        private ProgressionService _progression;
        private UpgradeService _upgrades;

        public event Action<RunState> StateChanged;
        public RunState State { get; private set; } = RunState.None;
        public int RunCoins => _runCoins;

        private void Start()
        {
            ServiceLocator.TryGet(out _economy);
            ServiceLocator.TryGet(out _progression);
            ServiceLocator.TryGet(out _upgrades);

            if (_crowd != null)
            {
                _crowd.CountChanged += OnCrowdCountChanged;
            }

            SetState(RunState.PreRun);
        }

        private void OnDestroy()
        {
            if (_crowd != null)
            {
                _crowd.CountChanged -= OnCrowdCountChanged;
            }
        }

        public void Configure(GlobalTuning tuning, LevelManager levelManager, CrowdManager crowd, GameplayUI gameplayUI)
        {
            _tuning = tuning;
            _levelManager = levelManager;
            _crowd = crowd;
            _gameplayUI = gameplayUI;
        }

        public void BeginRun()
        {
            if (State != RunState.PreRun)
            {
                return;
            }

            SetState(RunState.Running);
        }

        public void PauseForCombat()
        {
            if (State == RunState.Running)
            {
                SetState(RunState.CombatPaused);
            }
        }

        public void ResumeFromCombat()
        {
            if (State == RunState.CombatPaused)
            {
                SetState(RunState.Running);
            }
        }

        public void BeginFinishSequence()
        {
            if (State != RunState.Running && State != RunState.CombatPaused)
            {
                return;
            }

            SetState(RunState.FinishSequence);
            if (_crowd != null)
            {
                VfxManager.SpawnFloatingText("BONUS RUN", _crowd.transform.position + Vector3.up * 2.8f, new Color(1f, 0.78f, 0.12f));
            }
        }

        public void AddBonusCoins(int amount, Vector3 worldPosition)
        {
            if (amount <= 0 || State != RunState.FinishSequence)
            {
                return;
            }

            AddPendingRewardCoins(amount, worldPosition);
        }

        public void AddCombatCoins(int amount, Vector3 worldPosition)
        {
            if (amount <= 0 || State == RunState.None || State == RunState.PreRun || State == RunState.Victory || State == RunState.Defeat)
            {
                return;
            }

            AddPendingRewardCoins(amount, worldPosition);
        }

        private void AddPendingRewardCoins(int amount, Vector3 worldPosition)
        {
            _bonusCoins += amount;
            VfxManager.SpawnFloatingText("+" + amount + " COINS", worldPosition + Vector3.up * 0.8f, new Color(1f, 0.78f, 0.12f));
        }

        public void WinRun()
        {
            if (_levelCompleted || State == RunState.Victory || State == RunState.Defeat)
            {
                return;
            }

            _levelCompleted = true;
            SetState(RunState.FinishSequence);

            LevelData currentLevel = _levelManager != null ? _levelManager.CurrentLevel : null;
            int baseReward = currentLevel != null ? currentLevel.baseCoinReward : 100;
            int bossBonus = 0;
            if (currentLevel != null && currentLevel.hasBoss)
            {
                bossBonus = currentLevel.bossDefinition != null
                    ? currentLevel.bossDefinition.GetCoinReward(currentLevel.levelIndex)
                    : (_tuning != null ? _tuning.bossCoinValue : 250);
            }
            int survivorBonus = (_crowd != null ? _crowd.Count : 0) * (_tuning != null ? _tuning.soldierCoinValue : 2);
            float coinMultiplier = _upgrades != null ? Mathf.Max(1f, _upgrades.GetValue(UpgradeType.CoinReward)) : 1f;
            _runCoins = Mathf.RoundToInt((baseReward + bossBonus + survivorBonus + _bonusCoins) * coinMultiplier);

            _economy?.AddCoins(_runCoins);
            _progression?.CompleteCurrentLevel();

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.Victory);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Success);
            }

            SetState(RunState.Victory);
            if (_crowd != null)
            {
                _crowd.PlayVictoryCelebration();
                VfxManager.Spawn(VfxCue.CoinBurst, _crowd.transform.position + Vector3.up * 1.4f);
                VfxManager.Spawn(VfxCue.VictoryBurst, _crowd.transform.position + Vector3.up * 1.6f);
                VfxManager.SpawnFloatingText("+" + _runCoins + " COINS", _crowd.transform.position + Vector3.up * 2.8f, new Color(1f, 0.78f, 0.12f));
            }
            CameraFollowRig.Shake(CameraShakeCue.Victory);
            _gameplayUI?.ShowVictory(_runCoins);
        }

        public void LoseRun()
        {
            if (State == RunState.Victory || State == RunState.Defeat)
            {
                return;
            }

            AwardDefeatCoins();
            SetState(RunState.Defeat);
            _gameplayUI?.ShowDefeat(_runCoins);

            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.Defeat);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Failure);
            }
        }

        private void OnCrowdCountChanged(int count)
        {
            if (count <= 0 && (State == RunState.Running || State == RunState.CombatPaused))
            {
                LoseRun();
            }
        }

        private void AwardDefeatCoins()
        {
            if (_bonusCoins <= 0 || _runCoins > 0)
            {
                return;
            }

            float coinMultiplier = _upgrades != null ? Mathf.Max(1f, _upgrades.GetValue(UpgradeType.CoinReward)) : 1f;
            _runCoins = Mathf.RoundToInt(_bonusCoins * coinMultiplier);
            _economy?.AddCoins(_runCoins);
        }

        private void SetState(RunState state)
        {
            State = state;
            StateChanged?.Invoke(state);
            _gameplayUI?.SetRunState(state);
        }
    }
}
