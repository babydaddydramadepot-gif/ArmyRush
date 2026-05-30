using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public sealed class LevelManager : MonoBehaviour
    {
        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private LevelData[] _levels;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private CrowdManager _crowd;
        [SerializeField] private GameObject _trackSegmentPrefab;
        [SerializeField] private GameObject _gatePrefab;
        [SerializeField] private GameObject _enemyGroupPrefab;
        [SerializeField] private GameObject _enemyUnitPrefab;
        [SerializeField] private GameObject _obstaclePrefab;
        [SerializeField] private GameObject _bossPrefab;
        [SerializeField] private GameObject _finishLinePrefab;
        [SerializeField] private GameObject _bonusCratePrefab;
        [SerializeField] private GameObject _bonusEndPrefab;
        [SerializeField] private Transform _levelRoot;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private ProgressionService _progression;
        private UpgradeService _upgrades;
        private LevelData _runtimeEndlessLevel;

        public LevelData CurrentLevel { get; private set; }
        public BossController ActiveBoss { get; private set; }

        private void Start()
        {
            ServiceLocator.TryGet(out _progression);
            ServiceLocator.TryGet(out _upgrades);
            BuildCurrentLevel();
        }

        public void Configure(
            GlobalTuning tuning,
            LevelData[] levels,
            PoolManager poolManager,
            CrowdManager crowd,
            GameObject trackSegmentPrefab,
            GameObject gatePrefab,
            GameObject enemyGroupPrefab,
            GameObject enemyUnitPrefab,
            GameObject obstaclePrefab,
            GameObject bossPrefab,
            GameObject finishLinePrefab,
            GameObject bonusCratePrefab,
            GameObject bonusEndPrefab,
            Transform levelRoot)
        {
            _tuning = tuning;
            _levels = levels;
            _poolManager = poolManager;
            _crowd = crowd;
            _trackSegmentPrefab = trackSegmentPrefab;
            _gatePrefab = gatePrefab;
            _enemyGroupPrefab = enemyGroupPrefab;
            _enemyUnitPrefab = enemyUnitPrefab;
            _obstaclePrefab = obstaclePrefab;
            _bossPrefab = bossPrefab;
            _finishLinePrefab = finishLinePrefab;
            _bonusCratePrefab = bonusCratePrefab;
            _bonusEndPrefab = bonusEndPrefab;
            _levelRoot = levelRoot;
        }

        public void BuildCurrentLevel()
        {
            ClearLevel();

            int desiredIndex = _progression != null ? _progression.CurrentLevelIndex : 1;
            CurrentLevel = SelectLevel(desiredIndex);
            if (CurrentLevel == null)
            {
                Debug.LogError("No LevelData assets assigned.");
                return;
            }

            int startingSoldiers = CurrentLevel.startingSoldiersOverride > 0 ? CurrentLevel.startingSoldiersOverride : (_tuning != null ? _tuning.defaultStartingSoldiers : 10);
            if (_upgrades != null)
            {
                startingSoldiers = Mathf.RoundToInt(Mathf.Max(startingSoldiers, _upgrades.GetValue(UpgradeType.StartingTroops)));
            }
            PrewarmLevelPools(startingSoldiers);
            _crowd?.SetCount(startingSoldiers);

            BuildTrack(CurrentLevel.trackLength + Mathf.Max(0f, CurrentLevel.bonusSectionLength));
            SpawnGates();
            SpawnEnemies();
            SpawnObstacles();
            SpawnBoss();
            SpawnFinish();
            SpawnBonusSection();
        }

        public LevelData PreviewLevelData(int levelIndex)
        {
            return SelectLevel(Mathf.Max(1, levelIndex));
        }

        private LevelData SelectLevel(int desiredIndex)
        {
            if (_levels == null || _levels.Length == 0)
            {
                return null;
            }

            int highestAuthoredLevel = GetHighestAuthoredLevelIndex();
            if (desiredIndex > highestAuthoredLevel)
            {
                return BuildEndlessLevel(desiredIndex, highestAuthoredLevel);
            }

            for (int i = 0; i < _levels.Length; i++)
            {
                if (_levels[i] != null && _levels[i].levelIndex == desiredIndex)
                {
                    return _levels[i];
                }
            }

            int wrapped = Mathf.Abs((desiredIndex - 1) % _levels.Length);
            return _levels[wrapped];
        }

        private int GetHighestAuthoredLevelIndex()
        {
            int highest = 0;
            for (int i = 0; i < _levels.Length; i++)
            {
                if (_levels[i] != null)
                {
                    highest = Mathf.Max(highest, _levels[i].levelIndex);
                }
            }

            return Mathf.Max(1, highest);
        }

        private LevelData BuildEndlessLevel(int desiredIndex, int highestAuthoredLevel)
        {
            if (_runtimeEndlessLevel == null)
            {
                _runtimeEndlessLevel = ScriptableObject.CreateInstance<LevelData>();
                _runtimeEndlessLevel.hideFlags = HideFlags.DontSave;
            }

            int overflow = Mathf.Max(1, desiredIndex - highestAuthoredLevel);
            float difficulty = 1f + overflow * 0.08f;
            float trackLength = Mathf.Min(270f, 190f + overflow * 4.5f);
            BossDefinition bossDefinition = FindEndlessBossDefinition(overflow);

            _runtimeEndlessLevel.name = "Runtime_Endless_Level_" + desiredIndex;
            _runtimeEndlessLevel.levelIndex = desiredIndex;
            _runtimeEndlessLevel.trackLength = trackLength;
            _runtimeEndlessLevel.startingSoldiersOverride = 0;
            _runtimeEndlessLevel.baseCoinReward = Mathf.RoundToInt(1200f + overflow * 95f);
            _runtimeEndlessLevel.difficultyRating = desiredIndex;
            _runtimeEndlessLevel.hasBoss = bossDefinition != null && desiredIndex % 5 == 0;
            _runtimeEndlessLevel.bossDefinition = _runtimeEndlessLevel.hasBoss ? bossDefinition : null;
            _runtimeEndlessLevel.bossHealth = _runtimeEndlessLevel.hasBoss ? bossDefinition.GetHealth(desiredIndex, 0) : 0;
            _runtimeEndlessLevel.bonusCrateCount = Mathf.Clamp(3 + overflow / 8, 3, 5);
            _runtimeEndlessLevel.bonusCrateHealth = Mathf.RoundToInt(120f * difficulty + overflow * 18f);
            _runtimeEndlessLevel.bonusCrateReward = Mathf.RoundToInt(130f + overflow * 12f);
            _runtimeEndlessLevel.bonusSectionLength = Mathf.Clamp(34f + overflow * 0.35f, 34f, 48f);
            _runtimeEndlessLevel.gates.Clear();
            _runtimeEndlessLevel.enemyGroups.Clear();
            _runtimeEndlessLevel.obstacles.Clear();

            AddEndlessGatePair(16f, GateOperation.Add, Mathf.RoundToInt(18f * difficulty), GateOperation.Multiply, overflow % 3 == 0 ? 3 : 2);
            AddEndlessEnemy(34f, 0f, Mathf.RoundToInt(28f * difficulty), Mathf.RoundToInt(16f * difficulty));
            AddEndlessGatePair(52f, overflow % 2 == 0 ? GateOperation.Subtract : GateOperation.Add, Mathf.RoundToInt(14f * difficulty), GateOperation.Add, Mathf.RoundToInt(26f * difficulty));
            AddEndlessObstacle(72f, overflow % 2 == 0 ? -1.45f : 1.45f, Mathf.RoundToInt(210f * difficulty), Mathf.RoundToInt(14f * difficulty), FindEndlessObstacleDefinition(overflow, 0));
            AddEndlessGatePair(94f, GateOperation.Multiply, overflow % 4 == 0 ? 3 : 2, GateOperation.Add, Mathf.RoundToInt(32f * difficulty));
            AddEndlessEnemy(118f, overflow % 2 == 0 ? 1.15f : -1.15f, Mathf.RoundToInt(42f * difficulty), Mathf.RoundToInt(18f * difficulty));
            AddEndlessObstacle(142f, -1.55f, Mathf.RoundToInt(250f * difficulty), Mathf.RoundToInt(16f * difficulty), FindEndlessObstacleDefinition(overflow, 1));
            AddEndlessObstacle(142f, 1.55f, Mathf.RoundToInt(270f * difficulty), Mathf.RoundToInt(16f * difficulty), FindEndlessObstacleDefinition(overflow, 2));

            if (_runtimeEndlessLevel.hasBoss)
            {
                AddEndlessGatePair(trackLength - 48f, GateOperation.Add, Mathf.RoundToInt(42f * difficulty), GateOperation.Multiply, overflow >= 10 ? 3 : 2);
            }
            else
            {
                AddEndlessGatePair(trackLength - 42f, GateOperation.Add, Mathf.RoundToInt(36f * difficulty), GateOperation.Multiply, 2);
                AddEndlessEnemy(trackLength - 22f, 0f, Mathf.RoundToInt(52f * difficulty), Mathf.RoundToInt(20f * difficulty));
            }

            return _runtimeEndlessLevel;
        }

        private BossDefinition FindEndlessBossDefinition(int overflow)
        {
            BossDefinition fallback = null;
            int bossIndex = 0;
            for (int i = 0; i < _levels.Length; i++)
            {
                BossDefinition definition = _levels[i] != null ? _levels[i].bossDefinition : null;
                if (definition == null)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = definition;
                }

                if (bossIndex == overflow / 5)
                {
                    return definition;
                }

                bossIndex++;
            }

            return fallback;
        }

        private ObstacleDefinition FindEndlessObstacleDefinition(int overflow, int salt)
        {
            List<ObstacleDefinition> definitions = new List<ObstacleDefinition>();
            for (int i = 0; i < _levels.Length; i++)
            {
                LevelData level = _levels[i];
                if (level == null || level.obstacles == null)
                {
                    continue;
                }

                for (int j = 0; j < level.obstacles.Count; j++)
                {
                    ObstacleDefinition definition = level.obstacles[j] != null ? level.obstacles[j].definition : null;
                    if (definition != null && !definitions.Contains(definition))
                    {
                        definitions.Add(definition);
                    }
                }
            }

            if (definitions.Count == 0)
            {
                return null;
            }

            return definitions[Mathf.Abs((overflow + salt) % definitions.Count)];
        }

        private void AddEndlessGatePair(float z, GateOperation leftOperation, int leftValue, GateOperation rightOperation, int rightValue)
        {
            _runtimeEndlessLevel.gates.Add(new GateSpawnData { z = z, x = -1.45f, operation = leftOperation, value = Mathf.Max(1, leftValue) });
            _runtimeEndlessLevel.gates.Add(new GateSpawnData { z = z, x = 1.45f, operation = rightOperation, value = Mathf.Max(1, rightValue) });
        }

        private void AddEndlessEnemy(float z, float x, int count, int healthPerUnit)
        {
            _runtimeEndlessLevel.enemyGroups.Add(new EnemyGroupSpawnData { z = z, x = x, count = Mathf.Max(1, count), healthPerUnit = Mathf.Max(1, healthPerUnit) });
        }

        private void AddEndlessObstacle(float z, float x, int health, int penalty, ObstacleDefinition definition)
        {
            _runtimeEndlessLevel.obstacles.Add(new ObstacleSpawnData
            {
                definition = definition,
                z = z,
                x = x,
                health = Mathf.Max(1, health),
                collisionPenalty = Mathf.Max(0, penalty),
                width = definition != null ? definition.width : 1.6f
            });
        }

        private void BuildTrack(float length)
        {
            if (_trackSegmentPrefab == null)
            {
                return;
            }

            const float segmentLength = 10f;
            int segmentCount = Mathf.CeilToInt(length / segmentLength);
            for (int i = 0; i < segmentCount; i++)
            {
                GameObject segment = Instantiate(_trackSegmentPrefab, new Vector3(0f, -0.05f, i * segmentLength + segmentLength * 0.5f), Quaternion.identity, _levelRoot);
                _spawned.Add(segment);
            }
        }

        private void SpawnGates()
        {
            if (_gatePrefab == null)
            {
                return;
            }

            foreach (GateSpawnData data in CurrentLevel.gates)
            {
                GameObject gateObject = Instantiate(_gatePrefab, new Vector3(data.x, 0f, data.z), Quaternion.identity, _levelRoot);
                GateController gate = gateObject.GetComponent<GateController>();
                gate?.Configure(data.operation, data.value);
                _spawned.Add(gateObject);
            }
        }

        private void SpawnEnemies()
        {
            if (_enemyGroupPrefab == null)
            {
                return;
            }

            RunManager runManager = FindAnyObjectByType<RunManager>();
            foreach (EnemyGroupSpawnData data in CurrentLevel.enemyGroups)
            {
                GameObject enemyObject = Instantiate(_enemyGroupPrefab, new Vector3(data.x, 0f, data.z), Quaternion.identity, _levelRoot);
                EnemyGroup enemy = enemyObject.GetComponent<EnemyGroup>();
                float rewardPerEnemy = (_tuning != null ? _tuning.enemyCoinValue : 2) + Mathf.Max(0, CurrentLevel.levelIndex - 1) * 0.25f;
                int reward = Mathf.RoundToInt(data.count * rewardPerEnemy);
                enemy?.Configure(data.count, data.healthPerUnit, _enemyUnitPrefab, _poolManager, reward, runManager);
                _spawned.Add(enemyObject);
            }
        }

        private void SpawnObstacles()
        {
            RunManager runManager = FindAnyObjectByType<RunManager>();
            foreach (ObstacleSpawnData data in CurrentLevel.obstacles)
            {
                ObstacleDefinition definition = data.definition;
                GameObject obstaclePrefab = definition != null && definition.prefab != null ? definition.prefab : _obstaclePrefab;
                if (obstaclePrefab == null)
                {
                    continue;
                }

                GameObject obstacleObject = Instantiate(obstaclePrefab, new Vector3(data.x, 0f, data.z), Quaternion.identity, _levelRoot);
                ObstacleController obstacle = obstacleObject.GetComponent<ObstacleController>();
                int health = definition != null ? definition.GetHealth(CurrentLevel.levelIndex, data.health) : Mathf.Max(1, data.health);
                int penalty = definition != null ? definition.GetCollisionPenalty(data.collisionPenalty) : Mathf.Max(0, data.collisionPenalty);
                int reward = definition != null ? definition.GetCoinReward(CurrentLevel.levelIndex, data.coinReward) : (_tuning != null ? _tuning.obstacleCoinValue : 12) + Mathf.Max(0, CurrentLevel.levelIndex - 1);
                obstacle?.Configure(health, penalty, reward, runManager);
                _spawned.Add(obstacleObject);
            }
        }

        private void SpawnBoss()
        {
            GameObject bossPrefab = CurrentLevel.bossDefinition != null && CurrentLevel.bossDefinition.bossPrefab != null
                ? CurrentLevel.bossDefinition.bossPrefab
                : _bossPrefab;
            if (bossPrefab == null || !CurrentLevel.hasBoss)
            {
                return;
            }

            GameObject bossObject = Instantiate(bossPrefab, new Vector3(0f, 0f, CurrentLevel.trackLength - 24f), Quaternion.identity, _levelRoot);
            BossController boss = bossObject.GetComponent<BossController>();
            boss?.Configure(CurrentLevel.bossDefinition, CurrentLevel.levelIndex, CurrentLevel.bossHealth, _crowd);
            ActiveBoss = boss;
            _spawned.Add(bossObject);
        }

        private void SpawnFinish()
        {
            if (_finishLinePrefab == null)
            {
                return;
            }

            GameObject finish = Instantiate(_finishLinePrefab, new Vector3(0f, 0f, CurrentLevel.trackLength), Quaternion.identity, _levelRoot);
            _spawned.Add(finish);
        }

        private void SpawnBonusSection()
        {
            if (CurrentLevel == null)
            {
                return;
            }

            int crateCount = Mathf.Max(0, CurrentLevel.bonusCrateCount);
            float sectionLength = Mathf.Max(12f, CurrentLevel.bonusSectionLength);
            if (_bonusCratePrefab != null && crateCount > 0)
            {
                float spacing = sectionLength / (crateCount + 1f);
                RunManager runManager = FindAnyObjectByType<RunManager>();
                for (int i = 0; i < crateCount; i++)
                {
                    float laneX = i % 3 == 0 ? 0f : i % 3 == 1 ? -1.45f : 1.45f;
                    float z = CurrentLevel.trackLength + spacing * (i + 1);
                    GameObject crateObject = Instantiate(_bonusCratePrefab, new Vector3(laneX, 0f, z), Quaternion.identity, _levelRoot);
                    BonusCrateController crate = crateObject.GetComponent<BonusCrateController>();
                    int health = CurrentLevel.bonusCrateHealth + CurrentLevel.levelIndex * 12 + i * 18;
                    int reward = CurrentLevel.bonusCrateReward + CurrentLevel.levelIndex * 6 + i * 10;
                    crate?.Configure(health, reward, runManager);
                    _spawned.Add(crateObject);
                }
            }

            if (_bonusEndPrefab != null)
            {
                GameObject end = Instantiate(_bonusEndPrefab, new Vector3(0f, 0f, CurrentLevel.trackLength + sectionLength), Quaternion.identity, _levelRoot);
                _spawned.Add(end);
            }
        }

        private void PrewarmLevelPools(int startingSoldiers)
        {
            _crowd?.PrewarmVisuals(startingSoldiers);

            if (_poolManager == null || _enemyUnitPrefab == null || CurrentLevel == null)
            {
                return;
            }

            int enemyVisualCount = 0;
            for (int i = 0; i < CurrentLevel.enemyGroups.Count; i++)
            {
                enemyVisualCount += Mathf.Min(CurrentLevel.enemyGroups[i].count, 80);
            }
            if (enemyVisualCount > 0)
            {
                _poolManager.Prewarm(_enemyUnitPrefab, enemyVisualCount);
            }
        }

        private void ClearLevel()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                {
                    Destroy(_spawned[i]);
                }
            }
            _spawned.Clear();
            ActiveBoss = null;
        }
    }
}
