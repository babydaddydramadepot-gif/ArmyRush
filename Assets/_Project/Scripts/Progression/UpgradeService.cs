using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public sealed class UpgradeService
    {
        private readonly SaveService _saveService;
        private readonly EconomyService _economyService;
        private readonly ProgressionService _progressionService;
        private readonly Dictionary<UpgradeType, UpgradeDefinition> _definitions = new Dictionary<UpgradeType, UpgradeDefinition>();

        public event Action<UpgradeType, int> UpgradePurchased;

        public UpgradeService(SaveService saveService, EconomyService economyService, ProgressionService progressionService, IEnumerable<UpgradeDefinition> definitions)
        {
            _saveService = saveService;
            _economyService = economyService;
            _progressionService = progressionService;

            if (definitions == null)
            {
                return;
            }

            foreach (UpgradeDefinition definition in definitions)
            {
                if (definition != null)
                {
                    _definitions[definition.type] = definition;
                }
            }
        }

        public IReadOnlyDictionary<UpgradeType, UpgradeDefinition> Definitions => _definitions;

        public int GetLevel(UpgradeType type)
        {
            return _saveService.Data.GetUpgradeLevel(type);
        }

        public int GetCost(UpgradeType type)
        {
            return TryGetDefinition(type, out UpgradeDefinition definition) ? definition.GetCost(GetLevel(type)) : 0;
        }

        public float GetValue(UpgradeType type)
        {
            return TryGetDefinition(type, out UpgradeDefinition definition) ? definition.GetValue(GetLevel(type)) : 0f;
        }

        public bool CanPurchase(UpgradeType type)
        {
            if (!TryGetDefinition(type, out UpgradeDefinition definition))
            {
                return false;
            }

            int level = GetLevel(type);
            return IsUnlocked(type) && !definition.IsMaxed(level) && _economyService.Coins >= definition.GetCost(level);
        }

        public bool IsUnlocked(UpgradeType type)
        {
            if (!TryGetDefinition(type, out UpgradeDefinition definition))
            {
                return false;
            }

            int currentLevelIndex = _progressionService != null ? _progressionService.CurrentLevelIndex : 1;
            return definition.IsUnlocked(currentLevelIndex);
        }

        public bool Purchase(UpgradeType type)
        {
            if (!TryGetDefinition(type, out UpgradeDefinition definition))
            {
                Debug.LogWarning($"Missing upgrade definition for {type}.");
                return false;
            }

            int level = GetLevel(type);
            if (!definition.IsUnlocked(_progressionService != null ? _progressionService.CurrentLevelIndex : 1))
            {
                return false;
            }
            if (definition.IsMaxed(level))
            {
                return false;
            }

            int cost = definition.GetCost(level);
            if (!_economyService.SpendCoins(cost))
            {
                return false;
            }

            int nextLevel = level + 1;
            _saveService.Data.SetUpgradeLevel(type, nextLevel);
            _saveService.Save();
            UpgradePurchased?.Invoke(type, nextLevel);
            return true;
        }

        public bool TryGetDefinition(UpgradeType type, out UpgradeDefinition definition)
        {
            return _definitions.TryGetValue(type, out definition) && definition != null;
        }
    }
}
