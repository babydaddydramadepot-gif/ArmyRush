using System;
using UnityEngine;

namespace ArmyRush
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public int currentLevelIndex = 1;
        public int coins;
        public int gems;
        public int startingTroopsLevel;
        public int damageLevel;
        public int fireRateLevel;
        public int coinRewardLevel;
        public int bossDamageLevel;
        public int obstacleDamageLevel;
        public int criticalChanceLevel;
        public int criticalDamageLevel;
        public bool tutorialCompleted;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public bool hapticsEnabled = true;

        public int GetUpgradeLevel(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.StartingTroops:
                    return startingTroopsLevel;
                case UpgradeType.Damage:
                    return damageLevel;
                case UpgradeType.FireRate:
                    return fireRateLevel;
                case UpgradeType.CoinReward:
                    return coinRewardLevel;
                case UpgradeType.BossDamage:
                    return bossDamageLevel;
                case UpgradeType.ObstacleDamage:
                    return obstacleDamageLevel;
                case UpgradeType.CriticalChance:
                    return criticalChanceLevel;
                case UpgradeType.CriticalDamage:
                    return criticalDamageLevel;
                default:
                    return 0;
            }
        }

        public void SetUpgradeLevel(UpgradeType type, int level)
        {
            level = Mathf.Max(0, level);
            switch (type)
            {
                case UpgradeType.StartingTroops:
                    startingTroopsLevel = level;
                    break;
                case UpgradeType.Damage:
                    damageLevel = level;
                    break;
                case UpgradeType.FireRate:
                    fireRateLevel = level;
                    break;
                case UpgradeType.CoinReward:
                    coinRewardLevel = level;
                    break;
                case UpgradeType.BossDamage:
                    bossDamageLevel = level;
                    break;
                case UpgradeType.ObstacleDamage:
                    obstacleDamageLevel = level;
                    break;
                case UpgradeType.CriticalChance:
                    criticalChanceLevel = level;
                    break;
                case UpgradeType.CriticalDamage:
                    criticalDamageLevel = level;
                    break;
            }
        }
    }
}
