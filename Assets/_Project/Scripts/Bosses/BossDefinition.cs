using UnityEngine;

namespace ArmyRush
{
    public enum BossAttackPattern
    {
        CannonVolley,
        SuppressionBurst,
        MissileStrike,
        ShockwaveSlam
    }

    [CreateAssetMenu(menuName = "ArmyRush/Bosses/Boss Definition", fileName = "SO_BossDefinition")]
    public sealed class BossDefinition : ScriptableObject
    {
        public string displayName = "TANK BOSS";
        public GameObject bossPrefab;
        public BossAttackPattern attackPattern = BossAttackPattern.CannonVolley;
        public int baseHealth = 1000;
        public int healthPerLevel = 80;
        public int collisionPenalty = 22;
        public int coinReward = 250;
        public int coinRewardPerLevel = 18;
        public float activationDistance = 18f;
        public float attackInterval = 2.35f;
        public float warningDuration = 0.72f;
        public float attackRadius = 1.45f;
        public float lateralRange = 3.2f;
        public int attackDamage = 7;
        public int attackDamagePerLevel = 1;
        public string warningText = "CANNON";
        public Color warningColor = new Color(1f, 0.28f, 0.1f);

        public int GetHealth(int levelIndex, int overrideHealth)
        {
            if (overrideHealth > 0)
            {
                return overrideHealth;
            }

            return Mathf.Max(1, baseHealth + Mathf.Max(0, levelIndex - 1) * healthPerLevel);
        }

        public int GetAttackDamage(int levelIndex)
        {
            return Mathf.Max(1, attackDamage + Mathf.Max(0, levelIndex - 1) * attackDamagePerLevel);
        }

        public int GetCoinReward(int levelIndex)
        {
            return Mathf.Max(0, coinReward + Mathf.Max(0, levelIndex - 1) * coinRewardPerLevel);
        }
    }
}
