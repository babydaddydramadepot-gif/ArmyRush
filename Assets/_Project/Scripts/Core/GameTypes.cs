using System;
using UnityEngine;

namespace ArmyRush
{
    public enum RunState
    {
        None,
        PreRun,
        Running,
        CombatPaused,
        FinishSequence,
        Victory,
        Defeat,
        Results
    }

    public enum GateOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        DamageBoost,
        FireRateBoost,
        CoinBoost
    }

    public enum UpgradeType
    {
        StartingTroops,
        Damage,
        FireRate,
        CoinReward,
        BossDamage,
        ObstacleDamage,
        CriticalChance,
        CriticalDamage
    }

    public enum CombatTargetKind
    {
        Enemy,
        Obstacle,
        Boss
    }

    [Serializable]
    public struct RangeFloat
    {
        public float min;
        public float max;

        public float Lerp(float t)
        {
            return Mathf.Lerp(min, max, Mathf.Clamp01(t));
        }
    }
}
