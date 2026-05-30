using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    [CreateAssetMenu(menuName = "ArmyRush/Levels/Level Data", fileName = "SO_LevelData")]
    public sealed class LevelData : ScriptableObject
    {
        public int levelIndex = 1;
        public float trackLength = 130f;
        public int startingSoldiersOverride = 0;
        public int baseCoinReward = 100;
        public int difficultyRating = 1;
        public bool hasBoss;
        public BossDefinition bossDefinition;
        public int bossHealth = 0;
        public int bonusCrateCount = 3;
        public int bonusCrateHealth = 80;
        public int bonusCrateReward = 25;
        public float bonusSectionLength = 34f;

        public List<GateSpawnData> gates = new List<GateSpawnData>();
        public List<EnemyGroupSpawnData> enemyGroups = new List<EnemyGroupSpawnData>();
        public List<ObstacleSpawnData> obstacles = new List<ObstacleSpawnData>();
    }

    [Serializable]
    public sealed class GateSpawnData
    {
        public float z;
        public float x;
        public GateOperation operation;
        public int value;
    }

    [Serializable]
    public sealed class EnemyGroupSpawnData
    {
        public float z;
        public float x;
        public int count;
        public int healthPerUnit = 10;
        public float width = 2.4f;
    }

    [Serializable]
    public sealed class ObstacleSpawnData
    {
        public float z;
        public float x;
        public int health = 100;
        public int collisionPenalty = 8;
        public float width = 1.6f;
    }
}
