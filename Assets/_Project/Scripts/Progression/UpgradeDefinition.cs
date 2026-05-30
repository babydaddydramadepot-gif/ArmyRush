using System;
using UnityEngine;

namespace ArmyRush
{
    [CreateAssetMenu(menuName = "ArmyRush/Progression/Upgrade Definition", fileName = "SO_Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        public UpgradeType type;
        public string displayName = "Upgrade";
        [TextArea] public string description = "Improves army power.";
        public int baseCost = 100;
        public float costGrowth = 1.15f;
        public float baseValue = 1f;
        public float valuePerLevel = 1f;
        public int maxLevel = 100;
        public int unlockLevel = 1;
        public float maxValue = 0f;
        public UpgradeValueMilestone[] valueMilestones = Array.Empty<UpgradeValueMilestone>();

        public int GetCost(int currentLevel)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, Mathf.Max(0, currentLevel))));
        }

        public float GetValue(int level)
        {
            level = Mathf.Max(0, level);
            float value = valueMilestones != null && valueMilestones.Length > 0
                ? EvaluateMilestoneValue(level)
                : baseValue + valuePerLevel * level;
            return maxValue > 0f ? Mathf.Min(maxValue, value) : value;
        }

        public bool IsMaxed(int level)
        {
            return maxLevel > 0 && level >= maxLevel;
        }

        public bool IsUnlocked(int currentLevelIndex)
        {
            return currentLevelIndex >= Mathf.Max(1, unlockLevel);
        }

        private float EvaluateMilestoneValue(int level)
        {
            int lowerLevel = 0;
            float lowerValue = baseValue;
            int upperLevel = int.MaxValue;
            float upperValue = baseValue;

            for (int i = 0; i < valueMilestones.Length; i++)
            {
                UpgradeValueMilestone milestone = valueMilestones[i];
                if (milestone.level <= level && milestone.level >= lowerLevel)
                {
                    lowerLevel = Mathf.Max(0, milestone.level);
                    lowerValue = milestone.value;
                }
                else if (milestone.level > level && milestone.level < upperLevel)
                {
                    upperLevel = milestone.level;
                    upperValue = milestone.value;
                }
            }

            if (upperLevel == int.MaxValue || upperLevel <= lowerLevel)
            {
                return lowerValue;
            }

            float t = Mathf.InverseLerp(lowerLevel, upperLevel, level);
            return Mathf.Lerp(lowerValue, upperValue, t);
        }
    }

    [Serializable]
    public struct UpgradeValueMilestone
    {
        public int level;
        public float value;
    }
}
