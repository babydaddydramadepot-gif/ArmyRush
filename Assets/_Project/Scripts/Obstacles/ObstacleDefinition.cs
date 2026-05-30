using UnityEngine;

namespace ArmyRush
{
    public enum ObstacleKind
    {
        Barricade,
        CrateStack,
        BarrelCluster,
        ConcreteBlock,
        MilitaryTruck,
        Turret,
        FuelTank
    }

    [CreateAssetMenu(menuName = "ArmyRush/Obstacles/Obstacle Definition", fileName = "SO_ObstacleDefinition")]
    public sealed class ObstacleDefinition : ScriptableObject
    {
        public string displayName = "Barricade";
        public ObstacleKind kind = ObstacleKind.Barricade;
        public GameObject prefab;
        public int baseHealth = 100;
        public int healthPerLevel = 28;
        public int collisionPenalty = 8;
        public int baseCoinReward = 12;
        public int coinRewardPerLevel = 1;
        public float width = 1.6f;
        public bool explosive;

        public int GetHealth(int levelIndex, int overrideHealth)
        {
            if (overrideHealth > 0)
            {
                return overrideHealth;
            }

            return Mathf.Max(1, baseHealth + Mathf.Max(0, levelIndex - 1) * Mathf.Max(0, healthPerLevel));
        }

        public int GetCollisionPenalty(int overridePenalty)
        {
            return overridePenalty > 0 ? overridePenalty : Mathf.Max(0, collisionPenalty);
        }

        public int GetCoinReward(int levelIndex, int overrideReward)
        {
            if (overrideReward > 0)
            {
                return overrideReward;
            }

            return Mathf.Max(0, baseCoinReward + Mathf.Max(0, levelIndex - 1) * Mathf.Max(0, coinRewardPerLevel));
        }
    }
}
