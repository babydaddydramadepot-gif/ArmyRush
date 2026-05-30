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
        [SerializeField] private Transform _levelRoot;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private ProgressionService _progression;
        private UpgradeService _upgrades;

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
            _crowd?.SetCount(startingSoldiers);

            BuildTrack(CurrentLevel.trackLength);
            SpawnGates();
            SpawnEnemies();
            SpawnObstacles();
            SpawnBoss();
            SpawnFinish();
        }

        private LevelData SelectLevel(int desiredIndex)
        {
            if (_levels == null || _levels.Length == 0)
            {
                return null;
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

            foreach (EnemyGroupSpawnData data in CurrentLevel.enemyGroups)
            {
                GameObject enemyObject = Instantiate(_enemyGroupPrefab, new Vector3(data.x, 0f, data.z), Quaternion.identity, _levelRoot);
                EnemyGroup enemy = enemyObject.GetComponent<EnemyGroup>();
                enemy?.Configure(data.count, data.healthPerUnit, _enemyUnitPrefab, _poolManager);
                _spawned.Add(enemyObject);
            }
        }

        private void SpawnObstacles()
        {
            if (_obstaclePrefab == null)
            {
                return;
            }

            foreach (ObstacleSpawnData data in CurrentLevel.obstacles)
            {
                GameObject obstacleObject = Instantiate(_obstaclePrefab, new Vector3(data.x, 0f, data.z), Quaternion.identity, _levelRoot);
                ObstacleController obstacle = obstacleObject.GetComponent<ObstacleController>();
                obstacle?.Configure(data.health, data.collisionPenalty);
                _spawned.Add(obstacleObject);
            }
        }

        private void SpawnBoss()
        {
            if (_bossPrefab == null || !CurrentLevel.hasBoss)
            {
                return;
            }

            GameObject bossObject = Instantiate(_bossPrefab, new Vector3(0f, 0f, CurrentLevel.trackLength - 24f), Quaternion.identity, _levelRoot);
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
