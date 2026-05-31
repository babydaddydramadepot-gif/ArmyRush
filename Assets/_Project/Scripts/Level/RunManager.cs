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
        private int _rallyAssistUses;
        private int _contactMercyUses;
        private float _nextRallyAssistTime;

        public event Action<RunState> StateChanged;
        public RunState State { get; private set; } = RunState.None;
        public int RunCoins => _runCoins;
        public int CurrentLevelIndex => _levelManager != null && _levelManager.CurrentLevel != null ? _levelManager.CurrentLevel.levelIndex : 1;

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

            _rallyAssistUses = 0;
            _contactMercyUses = 0;
            _nextRallyAssistTime = 0f;
            _runCoins = 0;
            _bonusCoins = 0;
            _levelCompleted = false;
            _gameplayUI?.SetRunCoinPreview(0);
            SetState(RunState.Running);
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.RunStart);
            }
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
            _gameplayUI?.SetRunCoinPreview(GetPreviewRewardCoins());
            VfxManager.SpawnFloatingText("+" + amount + " COINS", worldPosition + Vector3.up * 0.8f, new Color(1f, 0.78f, 0.12f));
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.CoinReward);
            }
        }

        private int GetPreviewRewardCoins()
        {
            float coinMultiplier = _upgrades != null ? Mathf.Max(1f, _upgrades.GetValue(UpgradeType.CoinReward)) : 1f;
            return Mathf.RoundToInt(Mathf.Max(0, _bonusCoins) * coinMultiplier);
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

            _gameplayUI?.SetRunCoinPreview(0);
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
            _gameplayUI?.SetRunCoinPreview(0);
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

        public bool TryApplyEarlyContactMercy(CrowdManager crowd, int remainingEnemies, Vector3 worldPosition)
        {
            if (_tuning == null || crowd == null || remainingEnemies <= 0)
            {
                return false;
            }

            if (State != RunState.Running && State != RunState.CombatPaused)
            {
                return false;
            }

            LevelData currentLevel = _levelManager != null ? _levelManager.CurrentLevel : null;
            if (currentLevel == null || currentLevel.levelIndex > Mathf.Max(0, _tuning.earlyContactMercyLevelLimit))
            {
                return false;
            }

            int maxUses = Mathf.Max(0, _tuning.earlyContactMercyMaxUsesPerRun);
            if (maxUses == 0 || _contactMercyUses >= maxUses)
            {
                return false;
            }

            int currentCount = crowd.Count;
            int minimumSoldiers = Mathf.Max(1, _tuning.earlyContactMercyMinimumSoldiers);
            int enemyBuffer = Mathf.Max(0, _tuning.earlyContactMercyEnemyBuffer);
            if (currentCount < minimumSoldiers || remainingEnemies <= currentCount || remainingEnemies - currentCount > enemyBuffer)
            {
                return false;
            }

            int survivors = Mathf.Clamp(_tuning.earlyContactMercySurvivors, 1, currentCount);
            int loss = currentCount - survivors;
            _contactMercyUses++;
            if (loss > 0)
            {
                crowd.Remove(loss);
            }

            VfxManager.SpawnFloatingText("LAST STAND", worldPosition + Vector3.up * 1.2f, new Color(0.28f, 1f, 0.68f));
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Medium);
            }

            return true;
        }

        private void OnCrowdCountChanged(int count)
        {
            if (count <= 0 && (State == RunState.Running || State == RunState.CombatPaused))
            {
                LoseRun();
                return;
            }

            TryApplyEarlyRallyAssist(count);
        }

        private void TryApplyEarlyRallyAssist(int count)
        {
            if (_tuning == null || _crowd == null)
            {
                return;
            }

            if (State != RunState.Running && State != RunState.CombatPaused)
            {
                return;
            }

            LevelData currentLevel = _levelManager != null ? _levelManager.CurrentLevel : null;
            if (currentLevel == null || currentLevel.levelIndex > Mathf.Max(0, _tuning.earlyRallyAssistLevelLimit))
            {
                return;
            }

            int maxUses = Mathf.Max(0, _tuning.earlyRallyAssistMaxUsesPerRun);
            if (maxUses == 0 || _rallyAssistUses >= maxUses || Time.time < _nextRallyAssistTime)
            {
                return;
            }

            int minimum = Mathf.Max(1, _tuning.earlyRallyAssistMinimumSoldiers);
            int target = Mathf.Clamp(_tuning.earlyRallyAssistTargetSoldiers, minimum + 1, Mathf.Max(minimum + 1, _tuning.hardSoldierCap));
            if (count <= 0 || count >= minimum || count >= target)
            {
                return;
            }

            int amount = target - count;
            if (amount <= 0)
            {
                return;
            }

            _rallyAssistUses++;
            _nextRallyAssistTime = Time.time + Mathf.Max(0.1f, _tuning.earlyRallyAssistCooldown);
            _crowd.Add(amount);
            VfxManager.SpawnFloatingText("RALLY +" + amount, _crowd.transform.position + Vector3.up * 2.55f, new Color(0.28f, 1f, 0.68f));
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Light);
            }
        }

        private void AwardDefeatCoins()
        {
            if (_runCoins > 0)
            {
                return;
            }

            int pendingCoins = Mathf.Max(0, _bonusCoins);
            LevelData currentLevel = _levelManager != null ? _levelManager.CurrentLevel : null;
            if (currentLevel != null && _tuning != null && currentLevel.levelIndex <= Mathf.Max(0, _tuning.earlyDefeatRewardLevelLimit))
            {
                int consolation = Mathf.RoundToInt(Mathf.Max(0, currentLevel.baseCoinReward) * Mathf.Clamp01(_tuning.earlyDefeatRewardFraction));
                consolation = Mathf.Max(consolation, Mathf.Max(0, _tuning.earlyDefeatMinimumCoins));
                pendingCoins = Mathf.Max(pendingCoins, consolation);
            }

            if (pendingCoins <= 0)
            {
                return;
            }

            float coinMultiplier = _upgrades != null ? Mathf.Max(1f, _upgrades.GetValue(UpgradeType.CoinReward)) : 1f;
            _runCoins = Mathf.RoundToInt(pendingCoins * coinMultiplier);
            _economy?.AddCoins(_runCoins);
            if (_crowd != null)
            {
                VfxManager.SpawnFloatingText("+" + _runCoins + " COINS", _crowd.transform.position + Vector3.up * 2.2f, new Color(1f, 0.78f, 0.12f));
            }
        }

        private void SetState(RunState state)
        {
            State = state;
            StateChanged?.Invoke(state);
            _gameplayUI?.SetRunState(state);
        }
    }
}
