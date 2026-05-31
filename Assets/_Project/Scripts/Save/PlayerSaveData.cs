using System;
using UnityEngine;

namespace ArmyRush
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public const int CurrentSaveVersion = 1;

        public int saveVersion = CurrentSaveVersion;
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
        public int lastFailedLevelIndex;
        public int consecutiveLevelFailures;
        public bool tutorialCompleted;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public bool hapticsEnabled = true;

        public bool Normalize()
        {
            bool changed = false;
            changed |= SetIfChanged(ref saveVersion, Mathf.Max(1, saveVersion));
            changed |= SetIfChanged(ref currentLevelIndex, Mathf.Max(1, currentLevelIndex));
            changed |= SetIfChanged(ref coins, ClampCurrency(coins));
            changed |= SetIfChanged(ref gems, ClampCurrency(gems));
            changed |= SetIfChanged(ref startingTroopsLevel, Mathf.Max(0, startingTroopsLevel));
            changed |= SetIfChanged(ref damageLevel, Mathf.Max(0, damageLevel));
            changed |= SetIfChanged(ref fireRateLevel, Mathf.Max(0, fireRateLevel));
            changed |= SetIfChanged(ref coinRewardLevel, Mathf.Max(0, coinRewardLevel));
            changed |= SetIfChanged(ref bossDamageLevel, Mathf.Max(0, bossDamageLevel));
            changed |= SetIfChanged(ref obstacleDamageLevel, Mathf.Max(0, obstacleDamageLevel));
            changed |= SetIfChanged(ref criticalChanceLevel, Mathf.Max(0, criticalChanceLevel));
            changed |= SetIfChanged(ref criticalDamageLevel, Mathf.Max(0, criticalDamageLevel));
            changed |= SetIfChanged(ref lastFailedLevelIndex, Mathf.Max(0, lastFailedLevelIndex));
            changed |= SetIfChanged(ref consecutiveLevelFailures, Mathf.Clamp(consecutiveLevelFailures, 0, 50));
            if (lastFailedLevelIndex == 0 && consecutiveLevelFailures != 0)
            {
                consecutiveLevelFailures = 0;
                changed = true;
            }
            changed |= SetIfChanged(ref musicVolume, ClampVolume(musicVolume));
            changed |= SetIfChanged(ref sfxVolume, ClampVolume(sfxVolume));
            return changed;
        }

        public static int ClampCurrency(long value)
        {
            if (value <= 0)
            {
                return 0;
            }

            return value >= int.MaxValue ? int.MaxValue : (int)value;
        }

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

        private static float ClampVolume(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 1f;
            }

            return Mathf.Clamp01(value);
        }

        private static bool SetIfChanged(ref int current, int normalized)
        {
            if (current == normalized)
            {
                return false;
            }

            current = normalized;
            return true;
        }

        private static bool SetIfChanged(ref float current, float normalized)
        {
            if (Mathf.Approximately(current, normalized))
            {
                return false;
            }

            current = normalized;
            return true;
        }
    }
}
