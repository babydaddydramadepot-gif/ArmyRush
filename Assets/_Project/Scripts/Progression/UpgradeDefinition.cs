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

        public int GetCost(int currentLevel)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, Mathf.Max(0, currentLevel))));
        }

        public float GetValue(int level)
        {
            return baseValue + valuePerLevel * Mathf.Max(0, level);
        }

        public bool IsMaxed(int level)
        {
            return maxLevel > 0 && level >= maxLevel;
        }
    }
}
