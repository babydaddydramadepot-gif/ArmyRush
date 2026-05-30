using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    [CreateAssetMenu(menuName = "ArmyRush/Levels/Level Chunk Data", fileName = "SO_LevelChunk")]
    public sealed class LevelChunkData : ScriptableObject
    {
        public LevelChunkKind kind;
        public string displayName;
        public float length = 32f;
        public int difficultyRating = 1;
        public bool isBossLeadIn;
        public bool isRewardFocused;

        public List<GateSpawnData> gates = new List<GateSpawnData>();
        public List<EnemyGroupSpawnData> enemyGroups = new List<EnemyGroupSpawnData>();
        public List<ObstacleSpawnData> obstacles = new List<ObstacleSpawnData>();
    }

    public enum LevelChunkKind
    {
        IntroGates,
        GateEnemy,
        ObstacleCorridor,
        RiskRewardSplit,
        EliteEncounter,
        BossLeadIn
    }
}
