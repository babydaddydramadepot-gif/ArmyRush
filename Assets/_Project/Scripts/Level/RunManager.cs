using System;
using System.Globalization;
using UnityEngine;

namespace ArmyRush
{
    public sealed class RunManager : MonoBehaviour
    {
        private enum RunBoostHudKind
        {
            None,
            Damage,
            FireRate,
            Coins,
            Power
        }

        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private CrowdManager _crowd;
        [SerializeField] private GameplayUI _gameplayUI;

        private int _runCoins;
        private int _bonusCoins;
        private int _combatRewardCoins;
        private int _bonusRunCoins;
        private int _lastDefeatConsolationCoins;
        private bool _levelCompleted;
        private EconomyService _economy;
        private ProgressionService _progression;
        private UpgradeService _upgrades;
        private int _rallyAssistUses;
        private int _contactMercyUses;
        private float _nextRallyAssistTime;
        private float _damageBoostMultiplier = 1f;
        private float _fireRateBoostMultiplier = 1f;
        private float _coinBoostMultiplier = 1f;
        private float _damageBoostEndTime;
        private float _fireRateBoostEndTime;
        private float _coinBoostEndTime;
        private float _damageBoostDuration;
        private float _fireRateBoostDuration;
        private float _coinBoostDuration;
        private bool _boostHudVisible;
        private bool _coinBoostPreviewWasActive;
        private RunBoostHudKind _boostHudKind = RunBoostHudKind.None;
        private int _boostHudSeconds = -1;
        private int _boostHudPercent = -1;
        private int _boostHudActiveCount = -1;
        private string _boostHudLabel = string.Empty;

        public event Action<RunState> StateChanged;
        public RunState State { get; private set; } = RunState.None;
        public int RunCoins => _runCoins;
        public int CurrentLevelIndex => _levelManager != null && _levelManager.CurrentLevel != null ? _levelManager.CurrentLevel.levelIndex : 1;
        public float ActiveDamageBoostMultiplier => GetActiveBoostMultiplier(_damageBoostMultiplier, _damageBoostEndTime);
        public float ActiveFireRateBoostMultiplier => GetActiveBoostMultiplier(_fireRateBoostMultiplier, _fireRateBoostEndTime);
        public float ActiveCoinBoostMultiplier => GetActiveBoostMultiplier(_coinBoostMultiplier, _coinBoostEndTime);

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

        private void Update()
        {
            RefreshRunBoostIndicator();
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
            _combatRewardCoins = 0;
            _bonusRunCoins = 0;
            _lastDefeatConsolationCoins = 0;
            _levelCompleted = false;
            ResetRunBoosts();
            ApplyOnboardingCombatBoost();
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

            _bonusRunCoins += amount;
            AddPendingRewardCoins(amount, worldPosition);
        }

        public void AddCombatCoins(int amount, Vector3 worldPosition)
        {
            if (amount <= 0 || State == RunState.None || State == RunState.PreRun || State == RunState.Victory || State == RunState.Defeat)
            {
                return;
            }

            _combatRewardCoins += amount;
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
            return Mathf.RoundToInt(Mathf.Max(0, _bonusCoins) * coinMultiplier * ActiveCoinBoostMultiplier);
        }

        public void ApplyRunBoostGate(GateOperation operation, int value, Vector3 worldPosition)
        {
            if (State != RunState.Running && State != RunState.CombatPaused && State != RunState.FinishSequence)
            {
                return;
            }

            float boost = CalculateRunBoostMultiplier(value);
            float duration = Mathf.Max(1f, _tuning != null ? _tuning.runBoostGateDuration : 10f);
            float endTime = Time.time + duration;
            string label;
            switch (operation)
            {
                case GateOperation.DamageBoost:
                    _damageBoostMultiplier = Mathf.Max(ActiveDamageBoostMultiplier, boost);
                    _damageBoostEndTime = endTime;
                    _damageBoostDuration = duration;
                    label = "DAMAGE +" + Mathf.Max(1, value) + "%";
                    break;
                case GateOperation.FireRateBoost:
                    _fireRateBoostMultiplier = Mathf.Max(ActiveFireRateBoostMultiplier, boost);
                    _fireRateBoostEndTime = endTime;
                    _fireRateBoostDuration = duration;
                    label = "FIRE +" + Mathf.Max(1, value) + "%";
                    break;
                case GateOperation.CoinBoost:
                    _coinBoostMultiplier = Mathf.Max(ActiveCoinBoostMultiplier, boost);
                    _coinBoostEndTime = endTime;
                    _coinBoostDuration = duration;
                    _gameplayUI?.SetRunCoinPreview(GetPreviewRewardCoins());
                    label = "COINS +" + Mathf.Max(1, value) + "%";
                    break;
                default:
                    return;
            }

            VfxManager.SpawnFloatingText(label, worldPosition + Vector3.up * 1.85f, new Color(0.32f, 0.92f, 1f));
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(HapticCue.Light);
            }

            RefreshRunBoostIndicator();
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
            float totalMultiplier = coinMultiplier * ActiveCoinBoostMultiplier;
            _runCoins = Mathf.RoundToInt((baseReward + bossBonus + survivorBonus + _bonusCoins) * totalMultiplier);
            string rewardBreakdown = BuildVictoryRewardBreakdown(baseReward, survivorBonus, _combatRewardCoins, _bonusRunCoins, bossBonus, totalMultiplier);

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
            _gameplayUI?.ShowVictory(_runCoins, rewardBreakdown);
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
            _gameplayUI?.ShowDefeat(_runCoins, BuildDefeatRewardBreakdown());

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
            _lastDefeatConsolationCoins = 0;
            if (currentLevel != null && _tuning != null && currentLevel.levelIndex <= Mathf.Max(0, _tuning.earlyDefeatRewardLevelLimit))
            {
                int consolation = Mathf.RoundToInt(Mathf.Max(0, currentLevel.baseCoinReward) * Mathf.Clamp01(_tuning.earlyDefeatRewardFraction));
                consolation = Mathf.Max(consolation, Mathf.Max(0, _tuning.earlyDefeatMinimumCoins));
                _lastDefeatConsolationCoins = Mathf.Max(0, consolation);
                pendingCoins = Mathf.Max(pendingCoins, consolation);
            }

            if (pendingCoins <= 0)
            {
                return;
            }

            float coinMultiplier = _upgrades != null ? Mathf.Max(1f, _upgrades.GetValue(UpgradeType.CoinReward)) : 1f;
            _runCoins = Mathf.RoundToInt(pendingCoins * coinMultiplier * ActiveCoinBoostMultiplier);
            _economy?.AddCoins(_runCoins);
            if (_crowd != null)
            {
                VfxManager.SpawnFloatingText("+" + _runCoins + " COINS", _crowd.transform.position + Vector3.up * 2.2f, new Color(1f, 0.78f, 0.12f));
            }
        }

        private string BuildVictoryRewardBreakdown(int baseReward, int survivorBonus, int combatCoins, int bonusRunCoins, int bossBonus, float multiplier)
        {
            string result = "BASE " + FormatCompactCoins(baseReward);
            if (combatCoins > 0)
            {
                result += "  LOOT " + FormatCompactCoins(combatCoins);
            }
            if (bonusRunCoins > 0)
            {
                result += "  BONUS " + FormatCompactCoins(bonusRunCoins);
            }
            if (survivorBonus > 0)
            {
                result += "  SURV " + FormatCompactCoins(survivorBonus);
            }
            if (bossBonus > 0)
            {
                result += "  BOSS " + FormatCompactCoins(bossBonus);
            }
            if (multiplier > 1.01f)
            {
                result += "  x" + multiplier.ToString("0.##", CultureInfo.InvariantCulture);
            }

            return result;
        }

        private string BuildDefeatRewardBreakdown()
        {
            if (_runCoins <= 0)
            {
                return "UPGRADE AND TRY AGAIN";
            }

            string result = string.Empty;
            if (_bonusCoins > 0)
            {
                result = "LOOT " + FormatCompactCoins(_bonusCoins);
            }
            if (_lastDefeatConsolationCoins > _bonusCoins)
            {
                result = string.IsNullOrEmpty(result)
                    ? "CONSOLATION " + FormatCompactCoins(_lastDefeatConsolationCoins)
                    : result + "  SAFE FLOOR " + FormatCompactCoins(_lastDefeatConsolationCoins);
            }

            return string.IsNullOrEmpty(result) ? "CONSOLATION " + FormatCompactCoins(_runCoins) : result;
        }

        private static string FormatCompactCoins(int amount)
        {
            amount = Mathf.Max(0, amount);
            if (amount >= 1000000)
            {
                return (Mathf.FloorToInt(amount / 100000f) / 10f).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            }
            if (amount >= 10000)
            {
                return (Mathf.FloorToInt(amount / 100f) / 10f).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        private void ResetRunBoosts()
        {
            _damageBoostMultiplier = 1f;
            _fireRateBoostMultiplier = 1f;
            _coinBoostMultiplier = 1f;
            _damageBoostEndTime = 0f;
            _fireRateBoostEndTime = 0f;
            _coinBoostEndTime = 0f;
            _damageBoostDuration = 0f;
            _fireRateBoostDuration = 0f;
            _coinBoostDuration = 0f;
            _coinBoostPreviewWasActive = false;
            HideRunBoostIndicator();
        }

        private void ApplyOnboardingCombatBoost()
        {
            if (_tuning == null || _crowd == null)
            {
                return;
            }

            LevelData currentLevel = _levelManager != null ? _levelManager.CurrentLevel : null;
            if (currentLevel == null || currentLevel.levelIndex > Mathf.Max(0, _tuning.onboardingCombatBoostLevelLimit))
            {
                return;
            }

            float duration = Mathf.Max(0f, _tuning.onboardingCombatBoostDuration);
            float damageBoost = Mathf.Max(1f, _tuning.onboardingCombatDamageMultiplier);
            float fireRateBoost = Mathf.Max(1f, _tuning.onboardingCombatFireRateMultiplier);
            if (duration <= 0f || (damageBoost <= 1f && fireRateBoost <= 1f))
            {
                return;
            }

            float endTime = Time.time + duration;
            _damageBoostMultiplier = Mathf.Max(_damageBoostMultiplier, damageBoost);
            _fireRateBoostMultiplier = Mathf.Max(_fireRateBoostMultiplier, fireRateBoost);
            _damageBoostEndTime = Mathf.Max(_damageBoostEndTime, endTime);
            _fireRateBoostEndTime = Mathf.Max(_fireRateBoostEndTime, endTime);
            _damageBoostDuration = Mathf.Max(_damageBoostDuration, duration);
            _fireRateBoostDuration = Mathf.Max(_fireRateBoostDuration, duration);
            VfxManager.SpawnFloatingText("POWER START", _crowd.transform.position + Vector3.up * 2.7f, new Color(0.28f, 1f, 0.82f));
            RefreshRunBoostIndicator();
        }

        private float CalculateRunBoostMultiplier(int value)
        {
            float boost = 1f + Mathf.Max(1, value) / 100f;
            float maxMultiplier = _tuning != null ? Mathf.Max(1f, _tuning.maxRunBoostGateMultiplier) : 1.75f;
            return Mathf.Clamp(boost, 1f, maxMultiplier);
        }

        private static float GetActiveBoostMultiplier(float multiplier, float endTime)
        {
            return Time.time <= endTime ? Mathf.Max(1f, multiplier) : 1f;
        }

        private void RefreshRunBoostIndicator()
        {
            if (_gameplayUI == null)
            {
                return;
            }

            bool canShowBoost = State == RunState.Running || State == RunState.CombatPaused || State == RunState.FinishSequence;
            if (!canShowBoost)
            {
                HideRunBoostIndicator();
                return;
            }

            float damageRemaining = GetActiveBoostRemaining(_damageBoostMultiplier, _damageBoostEndTime);
            float fireRateRemaining = GetActiveBoostRemaining(_fireRateBoostMultiplier, _fireRateBoostEndTime);
            float coinRemaining = GetActiveBoostRemaining(_coinBoostMultiplier, _coinBoostEndTime);
            bool damageActive = damageRemaining > 0f;
            bool fireRateActive = fireRateRemaining > 0f;
            bool coinActive = coinRemaining > 0f;

            if (coinActive != _coinBoostPreviewWasActive)
            {
                _gameplayUI.SetRunCoinPreview(GetPreviewRewardCoins());
                _coinBoostPreviewWasActive = coinActive;
            }

            int activeCount = (damageActive ? 1 : 0) + (fireRateActive ? 1 : 0) + (coinActive ? 1 : 0);
            if (activeCount == 0)
            {
                HideRunBoostIndicator();
                return;
            }

            RunBoostHudKind kind;
            float remaining;
            float multiplier;
            float duration;
            Color color;
            if (activeCount > 1)
            {
                kind = RunBoostHudKind.Power;
                remaining = Mathf.Max(damageRemaining, fireRateRemaining, coinRemaining);
                multiplier = Mathf.Max(_damageBoostMultiplier, _fireRateBoostMultiplier, _coinBoostMultiplier);
                duration = Mathf.Max(
                    GetActiveBoostDuration(damageActive, _damageBoostDuration),
                    GetActiveBoostDuration(fireRateActive, _fireRateBoostDuration),
                    GetActiveBoostDuration(coinActive, _coinBoostDuration));
                color = new Color(0.28f, 1f, 0.82f);
            }
            else if (fireRateActive)
            {
                kind = RunBoostHudKind.FireRate;
                remaining = fireRateRemaining;
                multiplier = _fireRateBoostMultiplier;
                duration = GetActiveBoostDuration(true, _fireRateBoostDuration);
                color = new Color(0.32f, 0.92f, 1f);
            }
            else if (damageActive)
            {
                kind = RunBoostHudKind.Damage;
                remaining = damageRemaining;
                multiplier = _damageBoostMultiplier;
                duration = GetActiveBoostDuration(true, _damageBoostDuration);
                color = new Color(1f, 0.45f, 0.24f);
            }
            else
            {
                kind = RunBoostHudKind.Coins;
                remaining = coinRemaining;
                multiplier = _coinBoostMultiplier;
                duration = GetActiveBoostDuration(true, _coinBoostDuration);
                color = new Color(1f, 0.78f, 0.12f);
            }

            int seconds = Mathf.Max(1, Mathf.CeilToInt(remaining));
            int percent = Mathf.Max(1, Mathf.RoundToInt((Mathf.Max(1f, multiplier) - 1f) * 100f));
            if (!_boostHudVisible || kind != _boostHudKind || seconds != _boostHudSeconds || percent != _boostHudPercent || activeCount != _boostHudActiveCount)
            {
                _boostHudLabel = BuildRunBoostHudLabel(kind, percent, seconds, activeCount);
                _boostHudKind = kind;
                _boostHudSeconds = seconds;
                _boostHudPercent = percent;
                _boostHudActiveCount = activeCount;
            }

            _boostHudVisible = true;
            _gameplayUI.SetBoostIndicator(true, _boostHudLabel, remaining / duration, color);
        }

        private void HideRunBoostIndicator()
        {
            if (!_boostHudVisible)
            {
                _boostHudKind = RunBoostHudKind.None;
                _boostHudSeconds = -1;
                _boostHudPercent = -1;
                _boostHudActiveCount = -1;
                return;
            }

            _boostHudVisible = false;
            _boostHudKind = RunBoostHudKind.None;
            _boostHudSeconds = -1;
            _boostHudPercent = -1;
            _boostHudActiveCount = -1;
            _boostHudLabel = string.Empty;
            _gameplayUI?.SetBoostIndicator(false, string.Empty, 0f, Color.clear);
        }

        private static string BuildRunBoostHudLabel(RunBoostHudKind kind, int percentOrCount, int seconds, int activeCount)
        {
            switch (kind)
            {
                case RunBoostHudKind.FireRate:
                    return "FIRE +" + percentOrCount + "%  " + seconds + "s";
                case RunBoostHudKind.Damage:
                    return "DMG +" + percentOrCount + "%  " + seconds + "s";
                case RunBoostHudKind.Coins:
                    return "COIN +" + percentOrCount + "%  " + seconds + "s";
                case RunBoostHudKind.Power:
                    return "POWER +" + percentOrCount + "%  " + seconds + "s";
                default:
                    return string.Empty;
            }
        }

        private static float GetActiveBoostRemaining(float multiplier, float endTime)
        {
            return multiplier > 1f ? Mathf.Max(0f, endTime - Time.time) : 0f;
        }

        private static float GetActiveBoostDuration(bool active, float duration)
        {
            return active ? Mathf.Max(1f, duration) : 1f;
        }

        private void SetState(RunState state)
        {
            State = state;
            StateChanged?.Invoke(state);
            _gameplayUI?.SetRunState(state);
        }
    }
}
