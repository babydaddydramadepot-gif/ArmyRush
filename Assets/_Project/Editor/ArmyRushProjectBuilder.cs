using System.Collections.Generic;
using System.Linq;
using ArmyRush;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ArmyRushProjectBuilder
{
    private const string Root = "Assets/_Project";
    private const string ScenePath = Root + "/Scenes";
    private const string PrefabPath = Root + "/Prefabs";
    private const string MaterialPath = Root + "/Art/Materials";
    private const string MeshPath = Root + "/Art/Models/Generated";
    private const string LevelDataPath = Root + "/ScriptableObjects/Levels";
    private const string UpgradeDataPath = Root + "/ScriptableObjects/Upgrades";
    private const string TuningPath = Root + "/ScriptableObjects/Tuning";

    [MenuItem("ArmyRush/Build Production Foundation")]
    public static void BuildProductionFoundation()
    {
        CreateFolders();
        AssetDatabase.Refresh();

        MaterialSet materials = CreateMaterials();
        MeshSet meshes = CreateMeshes();
        GlobalTuning tuning = CreateTuning();
        UpgradeDefinition[] upgrades = CreateUpgrades();
        PrefabSet prefabs = CreatePrefabs(materials, meshes);
        LevelData[] levels = CreateLevels();

        CreateBootScene(upgrades);
        CreateMainMenuScene(upgrades);
        CreateGameScene(tuning, upgrades, prefabs, levels);
        ConfigureBuildSettings();
        ConfigurePlayerSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ArmyRush production foundation generated.");
    }

    [MenuItem("ArmyRush/Validate Production Foundation")]
    public static void ValidateProductionFoundation()
    {
        List<string> failures = new List<string>();
        ValidatePrefabFolder(failures);
        ValidateScene(ScenePath + "/Boot.unity", failures, ValidateBootScene);
        ValidateScene(ScenePath + "/MainMenu.unity", failures, ValidateMainMenuScene);
        ValidateScene(ScenePath + "/Game.unity", failures, ValidateGameScene);

        if (failures.Count > 0)
        {
            string message = "ArmyRush validation failed:\n" + string.Join("\n", failures);
            Debug.LogError(message);
            throw new System.Exception(message);
        }

        Debug.Log("ArmyRush production foundation validation passed.");
    }

    private static void CreateFolders()
    {
        string[] folders =
        {
            Root,
            Root + "/Art",
            Root + "/Art/Characters",
            Root + "/Art/Enemies",
            Root + "/Art/Bosses",
            Root + "/Art/Obstacles",
            Root + "/Art/Environment",
            Root + "/Art/UI",
            Root + "/Art/VFX",
            MaterialPath,
            MeshPath,
            Root + "/Audio",
            Root + "/Audio/Music",
            Root + "/Audio/SFX",
            PrefabPath,
            PrefabPath + "/Core",
            PrefabPath + "/Player",
            PrefabPath + "/Enemies",
            PrefabPath + "/Gates",
            PrefabPath + "/Obstacles",
            PrefabPath + "/Bosses",
            PrefabPath + "/UI",
            PrefabPath + "/VFX",
            PrefabPath + "/Levels",
            ScenePath,
            Root + "/ScriptableObjects",
            LevelDataPath,
            UpgradeDataPath,
            TuningPath,
            Root + "/ScriptableObjects/Economy",
            Root + "/ScriptableObjects/Enemies",
            Root + "/ScriptableObjects/Obstacles",
            Root + "/ScriptableObjects/Bosses",
            Root + "/Settings",
            Root + "/Shaders",
            Root + "/Tests"
        };

        foreach (string folder in folders)
        {
            EnsureFolder(folder);
        }
    }

    private static MaterialSet CreateMaterials()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        return new MaterialSet
        {
            playerBlue = CreateMaterial("MAT_PlayerBlue", shader, new Color(0.05f, 0.36f, 1f), 0f),
            playerNavy = CreateMaterial("MAT_PlayerNavy", shader, new Color(0.02f, 0.09f, 0.22f), 0f),
            enemyRed = CreateMaterial("MAT_EnemyRed", shader, new Color(1f, 0.12f, 0.09f), 0f),
            enemyCrimson = CreateMaterial("MAT_EnemyCrimson", shader, new Color(0.32f, 0.02f, 0.04f), 0f),
            skin = CreateMaterial("MAT_StylizedSkin", shader, new Color(1f, 0.74f, 0.48f), 0f),
            track = CreateMaterial("MAT_TrackGray", shader, new Color(0.46f, 0.5f, 0.56f), 0f),
            rail = CreateMaterial("MAT_RailWhite", shader, new Color(0.88f, 0.93f, 1f), 0f),
            ocean = CreateMaterial("MAT_OceanBlue", shader, new Color(0.04f, 0.58f, 0.9f), 0f),
            gatePositive = CreateMaterial("MAT_GatePositive", shader, new Color(0.05f, 0.95f, 0.72f), 0.2f),
            gateNegative = CreateMaterial("MAT_GateNegative", shader, new Color(1f, 0.24f, 0.12f), 0.2f),
            projectile = CreateMaterial("MAT_ProjectileYellow", shader, new Color(1f, 0.88f, 0.13f), 0f),
            coin = CreateMaterial("MAT_CoinGold", shader, new Color(1f, 0.68f, 0.05f), 0f),
            obstacle = CreateMaterial("MAT_ObstacleWood", shader, new Color(0.58f, 0.32f, 0.16f), 0f),
            obstacleMetal = CreateMaterial("MAT_ObstacleMetal", shader, new Color(0.34f, 0.38f, 0.42f), 0f),
            uiBlue = CreateMaterial("MAT_UIBlue", shader, new Color(0.1f, 0.32f, 1f), 0f)
        };
    }

    private static MeshSet CreateMeshes()
    {
        return new MeshSet
        {
            box = CreateMesh("MSH_Box", BuildBoxMesh()),
            panel = CreateMesh("MSH_GatePanel", BuildBoxMesh()),
            cylinder = CreateMesh("MSH_Cylinder12", BuildCylinderMesh(12)),
            road = CreateMesh("MSH_RoadSegment", BuildBoxMesh()),
            wedge = CreateMesh("MSH_Wedge", BuildWedgeMesh())
        };
    }

    private static GlobalTuning CreateTuning()
    {
        string path = TuningPath + "/SO_GlobalTuning.asset";
        GlobalTuning tuning = AssetDatabase.LoadAssetAtPath<GlobalTuning>(path);
        if (tuning == null)
        {
            tuning = ScriptableObject.CreateInstance<GlobalTuning>();
            AssetDatabase.CreateAsset(tuning, path);
        }
        EditorUtility.SetDirty(tuning);
        return tuning;
    }

    private static UpgradeDefinition[] CreateUpgrades()
    {
        return new[]
        {
            CreateUpgrade(UpgradeType.StartingTroops, "Starting Soldiers", "Begin each run with a larger squad.", 250, 1.15f, 10f, 0.65f),
            CreateUpgrade(UpgradeType.Damage, "Damage", "Increase all soldier weapon damage.", 100, 1.15f, 10f, 2.5f),
            CreateUpgrade(UpgradeType.FireRate, "Fire Rate", "Increase shots per second.", 150, 1.15f, 1f, 0.1f),
            CreateUpgrade(UpgradeType.CoinReward, "Coin Bonus", "Increase coins earned from every run.", 120, 1.15f, 1f, 0.02f),
            CreateUpgrade(UpgradeType.BossDamage, "Boss Damage", "Future boss damage scaling.", 300, 1.18f, 1f, 0.08f),
            CreateUpgrade(UpgradeType.ObstacleDamage, "Obstacle Damage", "Future obstacle damage scaling.", 220, 1.16f, 1f, 0.06f),
            CreateUpgrade(UpgradeType.CriticalChance, "Critical Chance", "Future critical hit unlock.", 400, 1.2f, 0f, 0.01f),
            CreateUpgrade(UpgradeType.CriticalDamage, "Critical Damage", "Future critical damage unlock.", 500, 1.2f, 1.5f, 0.04f)
        };
    }

    private static PrefabSet CreatePrefabs(MaterialSet materials, MeshSet meshes)
    {
        PrefabSet prefabs = new PrefabSet();
        prefabs.playerSoldier = CreateSoldierPrefab("PF_SoldierUnit_Blue", true, materials, meshes, PrefabPath + "/Player/PF_SoldierUnit_Blue.prefab");
        prefabs.enemySoldier = CreateSoldierPrefab("PF_EnemyUnit_Red", false, materials, meshes, PrefabPath + "/Enemies/PF_EnemyUnit_Red.prefab");
        prefabs.projectile = CreateProjectilePrefab(materials, meshes);
        prefabs.trackSegment = CreateTrackSegmentPrefab(materials, meshes);
        prefabs.gate = CreateGatePrefab(materials, meshes);
        prefabs.enemyGroup = CreateEnemyGroupPrefab(materials, meshes);
        prefabs.obstacle = CreateObstaclePrefab(materials, meshes);
        prefabs.bossTank = CreateBossTankPrefab(materials, meshes);
        prefabs.finishLine = CreateFinishLinePrefab(materials, meshes);
        prefabs.floatingText = CreateFloatingTextPrefab();
        return prefabs;
    }

    private static GameObject CreateSoldierPrefab(string name, bool player, MaterialSet materials, MeshSet meshes, string path)
    {
        GameObject root = new GameObject(name);
        PooledObject pooled = root.AddComponent<PooledObject>();
        SoldierUnitVisual visual = root.AddComponent<SoldierUnitVisual>();
        Transform bodyRoot = new GameObject("BodyRoot").transform;
        bodyRoot.SetParent(root.transform, false);

        Material primary = player ? materials.playerBlue : materials.enemyRed;
        Material dark = player ? materials.playerNavy : materials.enemyCrimson;

        AddMeshPart(bodyRoot, "Boots_L", meshes.box, dark, new Vector3(-0.12f, 0.12f, 0f), new Vector3(0.13f, 0.24f, 0.16f));
        AddMeshPart(bodyRoot, "Boots_R", meshes.box, dark, new Vector3(0.12f, 0.12f, 0f), new Vector3(0.13f, 0.24f, 0.16f));
        AddMeshPart(bodyRoot, "Leg_L", meshes.box, primary, new Vector3(-0.11f, 0.38f, 0f), new Vector3(0.12f, 0.32f, 0.14f));
        AddMeshPart(bodyRoot, "Leg_R", meshes.box, primary, new Vector3(0.11f, 0.38f, 0f), new Vector3(0.12f, 0.32f, 0.14f));
        AddMeshPart(bodyRoot, "Torso", meshes.box, primary, new Vector3(0f, 0.75f, 0f), new Vector3(0.42f, 0.46f, 0.24f));
        AddMeshPart(bodyRoot, "Vest", meshes.box, dark, new Vector3(0f, 0.78f, -0.03f), new Vector3(0.46f, 0.32f, 0.08f));
        AddMeshPart(bodyRoot, "Head", meshes.box, materials.skin, new Vector3(0f, 1.14f, 0f), new Vector3(0.28f, 0.28f, 0.25f));
        AddMeshPart(bodyRoot, "Helmet", meshes.wedge, dark, new Vector3(0f, 1.31f, 0f), new Vector3(0.34f, 0.16f, 0.29f));
        AddMeshPart(bodyRoot, "Arm_L", meshes.box, primary, new Vector3(-0.3f, 0.76f, -0.02f), new Vector3(0.1f, 0.38f, 0.12f));
        AddMeshPart(bodyRoot, "Arm_R", meshes.box, primary, new Vector3(0.3f, 0.76f, -0.02f), new Vector3(0.1f, 0.38f, 0.12f));
        Transform weapon = AddMeshPart(bodyRoot, "Rifle", meshes.box, dark, new Vector3(0f, 0.83f, 0.22f), new Vector3(0.12f, 0.12f, 0.55f));

        SetObject(visual, "_bodyRoot", bodyRoot);
        SetObject(visual, "_weaponRoot", weapon);

        GameObject prefab = SavePrefab(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateProjectilePrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_ProjectileTracer");
        root.AddComponent<PooledObject>();
        root.AddComponent<Projectile>();
        AddMeshPart(root.transform, "Tracer", meshes.box, materials.projectile, Vector3.zero, new Vector3(0.08f, 0.08f, 0.55f));
        GameObject prefab = SavePrefab(root, PrefabPath + "/VFX/PF_ProjectileTracer.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateTrackSegmentPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_TrackSegment");
        AddMeshPart(root.transform, "Road", meshes.road, materials.track, Vector3.zero, new Vector3(7.2f, 0.18f, 10f));
        AddMeshPart(root.transform, "LeftRail", meshes.box, materials.rail, new Vector3(-3.75f, 0.34f, 0f), new Vector3(0.18f, 0.5f, 10f));
        AddMeshPart(root.transform, "RightRail", meshes.box, materials.rail, new Vector3(3.75f, 0.34f, 0f), new Vector3(0.18f, 0.5f, 10f));
        AddMeshPart(root.transform, "LeftLaneStripe", meshes.box, materials.rail, new Vector3(-1.2f, 0.06f, 0f), new Vector3(0.05f, 0.02f, 8.8f));
        AddMeshPart(root.transform, "RightLaneStripe", meshes.box, materials.rail, new Vector3(1.2f, 0.06f, 0f), new Vector3(0.05f, 0.02f, 8.8f));
        GameObject prefab = SavePrefab(root, PrefabPath + "/Levels/PF_TrackSegment.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateGatePrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_Gate");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(2.4f, 3f, 0.55f);
        collider.center = new Vector3(0f, 1.4f, 0f);
        collider.isTrigger = true;
        GateController gate = root.AddComponent<GateController>();
        AddMeshPart(root.transform, "LeftPost", meshes.box, materials.rail, new Vector3(-1.18f, 1.08f, 0f), new Vector3(0.16f, 2.15f, 0.16f));
        AddMeshPart(root.transform, "RightPost", meshes.box, materials.rail, new Vector3(1.18f, 1.08f, 0f), new Vector3(0.16f, 2.15f, 0.16f));
        Transform panel = AddMeshPart(root.transform, "Panel", meshes.panel, materials.gatePositive, new Vector3(0f, 1.32f, 0f), new Vector3(2.12f, 1.45f, 0.08f));
        TextMesh label = CreateWorldText("TextLabel", root.transform, "+10", new Vector3(0f, 1.42f, -0.08f), 0.18f, Color.white);
        label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        SetObject(gate, "_label", label);
        SetObject(gate, "_panelRenderer", panel.GetComponent<Renderer>());
        GameObject prefab = SavePrefab(root, PrefabPath + "/Gates/PF_Gate.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateEnemyGroupPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_EnemyGroup");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(4.5f, 2.2f, 2.8f);
        collider.center = new Vector3(0f, 0.9f, 0f);
        collider.isTrigger = true;
        Damageable damageable = root.AddComponent<Damageable>();
        EnemyGroup group = root.AddComponent<EnemyGroup>();
        TextMesh label = CreateWorldText("CountLabel", root.transform, "20", new Vector3(0f, 2.05f, 0f), 0.15f, Color.white);
        label.gameObject.AddComponent<Billboard>();
        SetObject(damageable, "_label", label);
        SetObject(group, "_countLabel", label);
        SetObject(group, "_damageable", damageable);
        GameObject prefab = SavePrefab(root, PrefabPath + "/Enemies/PF_EnemyGroup.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateObstaclePrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_Obstacle_Barricade");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(2f, 1.5f, 1.1f);
        collider.center = new Vector3(0f, 0.75f, 0f);
        collider.isTrigger = true;
        Damageable damageable = root.AddComponent<Damageable>();
        ObstacleController obstacle = root.AddComponent<ObstacleController>();
        AddMeshPart(root.transform, "Crate_L", meshes.box, materials.obstacle, new Vector3(-0.48f, 0.38f, 0f), new Vector3(0.78f, 0.72f, 0.72f));
        AddMeshPart(root.transform, "Crate_R", meshes.box, materials.obstacle, new Vector3(0.48f, 0.38f, 0f), new Vector3(0.78f, 0.72f, 0.72f));
        AddMeshPart(root.transform, "MetalBand", meshes.box, materials.obstacleMetal, new Vector3(0f, 0.82f, -0.02f), new Vector3(1.85f, 0.16f, 0.82f));
        TextMesh label = CreateWorldText("HealthLabel", root.transform, "100", new Vector3(0f, 1.55f, 0f), 0.13f, Color.white);
        label.gameObject.AddComponent<Billboard>();
        SetObject(damageable, "_label", label);
        SetObject(obstacle, "_damageable", damageable);
        SetObject(obstacle, "_healthLabel", label);
        GameObject prefab = SavePrefab(root, PrefabPath + "/Obstacles/PF_Obstacle_Barricade.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateBossTankPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_Boss_Tank");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(3.4f, 2.1f, 3.2f);
        collider.center = new Vector3(0f, 0.95f, 0f);
        collider.isTrigger = true;
        Damageable damageable = root.AddComponent<Damageable>();
        BossController boss = root.AddComponent<BossController>();

        AddMeshPart(root.transform, "LeftTread", meshes.box, materials.obstacleMetal, new Vector3(-0.92f, 0.34f, 0f), new Vector3(0.62f, 0.45f, 2.55f));
        AddMeshPart(root.transform, "RightTread", meshes.box, materials.obstacleMetal, new Vector3(0.92f, 0.34f, 0f), new Vector3(0.62f, 0.45f, 2.55f));
        AddMeshPart(root.transform, "Hull", meshes.box, materials.enemyCrimson, new Vector3(0f, 0.82f, 0f), new Vector3(2.35f, 0.72f, 2.15f));
        AddMeshPart(root.transform, "Turret", meshes.wedge, materials.enemyRed, new Vector3(0f, 1.34f, 0.16f), new Vector3(1.35f, 0.58f, 1.25f));
        AddMeshPart(root.transform, "Cannon", meshes.box, materials.obstacleMetal, new Vector3(0f, 1.35f, -1.22f), new Vector3(0.22f, 0.22f, 1.7f));
        AddMeshPart(root.transform, "Antenna", meshes.box, materials.rail, new Vector3(0.62f, 1.85f, 0.32f), new Vector3(0.08f, 0.75f, 0.08f));

        TextMesh label = CreateWorldText("BossHealthLabel", root.transform, "TANK BOSS\n1000", new Vector3(0f, 2.25f, 0f), 0.12f, Color.white);
        label.gameObject.AddComponent<Billboard>();
        SetObject(damageable, "_label", label);
        SetObject(boss, "_damageable", damageable);
        SetObject(boss, "_healthLabel", label);

        GameObject prefab = SavePrefab(root, PrefabPath + "/Bosses/PF_Boss_Tank.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateFinishLinePrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_FinishLine");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(7f, 3f, 0.6f);
        collider.center = new Vector3(0f, 1.2f, 0f);
        collider.isTrigger = true;
        root.AddComponent<FinishLineTrigger>();
        AddMeshPart(root.transform, "FinishStrip", meshes.box, materials.coin, new Vector3(0f, 0.05f, 0f), new Vector3(7f, 0.08f, 0.55f));
        AddMeshPart(root.transform, "LeftBannerPost", meshes.box, materials.rail, new Vector3(-3.15f, 1.45f, 0f), new Vector3(0.15f, 2.9f, 0.15f));
        AddMeshPart(root.transform, "RightBannerPost", meshes.box, materials.rail, new Vector3(3.15f, 1.45f, 0f), new Vector3(0.15f, 2.9f, 0.15f));
        AddMeshPart(root.transform, "Banner", meshes.box, materials.gatePositive, new Vector3(0f, 2.55f, 0f), new Vector3(6.2f, 0.56f, 0.08f));
        TextMesh label = CreateWorldText("FinishLabel", root.transform, "FINISH", new Vector3(0f, 2.56f, -0.08f), 0.13f, Color.white);
        label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        GameObject prefab = SavePrefab(root, PrefabPath + "/Levels/PF_FinishLine.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateFloatingTextPrefab()
    {
        GameObject root = new GameObject("PF_FloatingText");
        root.AddComponent<PooledObject>();
        FloatingText floatingText = root.AddComponent<FloatingText>();
        TextMesh label = CreateWorldText("Label", root.transform, "+10", Vector3.zero, 0.16f, Color.white);
        SetObject(floatingText, "_label", label);
        GameObject prefab = SavePrefab(root, PrefabPath + "/VFX/PF_FloatingText.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static LevelData[] CreateLevels()
    {
        List<LevelData> levels = new List<LevelData>();
        for (int i = 1; i <= 20; i++)
        {
            string path = $"{LevelDataPath}/SO_Level_{i:000}.asset";
            LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.levelIndex = i;
            data.trackLength = Mathf.Lerp(120f, 210f, (i - 1) / 19f);
            data.startingSoldiersOverride = i == 1 ? 10 : 0;
            data.baseCoinReward = Mathf.RoundToInt(Mathf.Lerp(100f, 1200f, (i - 1) / 19f));
            data.difficultyRating = i;
            data.hasBoss = i == 5 || i == 10 || i == 15 || i == 20;
            data.bossHealth = data.hasBoss ? 500 + i * 120 : 0;
            data.gates.Clear();
            data.enemyGroups.Clear();
            data.obstacles.Clear();

            AddDesignedLevelData(data, i);
            EditorUtility.SetDirty(data);
            levels.Add(data);
        }

        return levels.ToArray();
    }

    private static void AddDesignedLevelData(LevelData data, int level)
    {
        AddGatePair(data, 16f, GateOperation.Add, 5 + level, GateOperation.Add, 10 + level * 2);
        AddEnemy(data, 32f, 0f, 8 + level * 3, 8 + level);
        AddGatePair(data, 47f, GateOperation.Multiply, level < 6 ? 2 : 3, GateOperation.Add, 18 + level * 3);
        AddObstacle(data, 64f, level % 2 == 0 ? -1.4f : 1.4f, 70 + level * 35, 5 + level);
        AddGatePair(data, 82f, level >= 3 ? GateOperation.Subtract : GateOperation.Add, level >= 3 ? 10 + level : 10, GateOperation.Multiply, 2);
        AddEnemy(data, 101f, level % 2 == 0 ? 1.2f : -1.2f, 16 + level * 5, 10 + level * 2);

        if (level >= 4)
        {
            AddObstacle(data, 118f, -1.6f, 110 + level * 45, 8 + level);
            AddObstacle(data, 118f, 1.6f, 130 + level * 48, 8 + level);
        }

        if (data.hasBoss)
        {
            AddGatePair(data, data.trackLength - 46f, GateOperation.Add, 30 + level * 3, GateOperation.Multiply, level >= 10 ? 3 : 2);
        }
        else
        {
            AddGatePair(data, data.trackLength - 38f, GateOperation.Add, 25 + level * 2, GateOperation.Multiply, 2);
            AddEnemy(data, data.trackLength - 20f, 0f, 22 + level * 4, 12 + level * 2);
        }
    }

    private static void AddGatePair(LevelData data, float z, GateOperation leftOperation, int leftValue, GateOperation rightOperation, int rightValue)
    {
        data.gates.Add(new GateSpawnData { z = z, x = -1.45f, operation = leftOperation, value = leftValue });
        data.gates.Add(new GateSpawnData { z = z, x = 1.45f, operation = rightOperation, value = rightValue });
    }

    private static void AddEnemy(LevelData data, float z, float x, int count, int hp)
    {
        data.enemyGroups.Add(new EnemyGroupSpawnData { z = z, x = x, count = count, healthPerUnit = hp });
    }

    private static void AddObstacle(LevelData data, float z, float x, int health, int penalty)
    {
        data.obstacles.Add(new ObstacleSpawnData { z = z, x = x, health = health, collisionPenalty = penalty });
    }

    private static void CreateBootScene(UpgradeDefinition[] upgrades)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootstrap = new GameObject("GameBootstrapper");
        GameBootstrapper bootstrapper = bootstrap.AddComponent<GameBootstrapper>();
        SetObjectArray(bootstrapper, "_upgradeDefinitions", upgrades);
        SetBool(bootstrapper, "_loadMainMenuOnStart", true);
        EditorSceneManager.SaveScene(scene, ScenePath + "/Boot.unity");
    }

    private static void CreateMainMenuScene(UpgradeDefinition[] upgrades)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateLighting();
        Camera camera = CreateCamera(new Vector3(0f, 5.6f, -8f), Quaternion.Euler(32f, 0f, 0f));
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.38f, 0.78f, 1f);

        Canvas canvas = CreateCanvas("MainMenuCanvas");
        GameObject safe = CreateSafeArea(canvas.transform);
        MainMenuUI menu = canvas.gameObject.AddComponent<MainMenuUI>();

        Text title = CreateUIText("Title", safe.transform, "ARMY RUSH", 86, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.08f, 0.16f, 0.32f), new Vector2(0.5f, 0.88f), new Vector2(760f, 120f));
        Text coins = CreateUIText("CoinsText", safe.transform, "0", 42, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.78f, 0.12f), new Vector2(0.82f, 0.955f), new Vector2(260f, 80f));
        Text level = CreateUIText("LevelText", safe.transform, "LEVEL 1", 40, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.78f), new Vector2(400f, 80f));

        Button play = CreateButton("PlayButton", safe.transform, "PLAY", new Vector2(0.5f, 0.19f), new Vector2(520f, 130f), new Color(0.05f, 0.78f, 0.35f));
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);

        UpgradeButtonView[] views = new UpgradeButtonView[4];
        UpgradeType[] types = { UpgradeType.Damage, UpgradeType.FireRate, UpgradeType.StartingTroops, UpgradeType.CoinReward };
        for (int i = 0; i < views.Length; i++)
        {
            Button button = CreateButton("Upgrade_" + types[i], safe.transform, string.Empty, new Vector2(0.5f, 0.62f - i * 0.105f), new Vector2(720f, 92f), new Color(0.08f, 0.28f, 0.95f));
            UpgradeButtonView view = button.gameObject.AddComponent<UpgradeButtonView>();
            Text titleText = CreateUIText("Title", button.transform, types[i].ToString().ToUpperInvariant(), 28, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.27f, 0.56f), new Vector2(340f, 40f));
            Text levelText = CreateUIText("Level", button.transform, "Lv. 0", 24, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.27f, 0.24f), new Vector2(220f, 34f));
            Text costText = CreateUIText("Cost", button.transform, "100", 30, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.83f, 0.2f), new Vector2(0.82f, 0.5f), new Vector2(180f, 54f));
            view.Configure(types[i]);
            SetObject(view, "_titleText", titleText);
            SetObject(view, "_levelText", levelText);
            SetObject(view, "_costText", costText);
            SetObject(view, "_button", button);
            views[i] = view;
        }

        SetObjectArray(menu, "_upgradeDefinitions", upgrades);
        SetObject(menu, "_coinsText", coins);
        SetObject(menu, "_levelText", level);
        SetObjectArray(menu, "_upgradeButtons", views);

        EditorSceneManager.SaveScene(scene, ScenePath + "/MainMenu.unity");
    }

    private static void CreateGameScene(GlobalTuning tuning, UpgradeDefinition[] upgrades, PrefabSet prefabs, LevelData[] levels)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateLighting();

        GameObject bootstrap = new GameObject("GameBootstrapper");
        GameBootstrapper bootstrapper = bootstrap.AddComponent<GameBootstrapper>();
        SetObjectArray(bootstrapper, "_upgradeDefinitions", upgrades);
        SetBool(bootstrapper, "_loadMainMenuOnStart", false);

        GameObject environment = new GameObject("EnvironmentRoot");
        AddMeshPart(environment.transform, "OceanPlane", AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath + "/MSH_Box.asset"), AssetDatabase.LoadAssetAtPath<Material>(MaterialPath + "/MAT_OceanBlue.mat"), new Vector3(0f, -0.24f, 105f), new Vector3(28f, 0.08f, 250f));

        GameObject levelRoot = new GameObject("LevelRoot");
        GameObject poolRoot = new GameObject("PoolRoot");
        PoolManager pool = poolRoot.AddComponent<PoolManager>();

        GameObject gameRoot = new GameObject("GameRoot");
        RunnerInputController input = gameRoot.AddComponent<RunnerInputController>();
        RunManager run = gameRoot.AddComponent<RunManager>();
        LevelManager levelManager = gameRoot.AddComponent<LevelManager>();
        VfxManager vfx = gameRoot.AddComponent<VfxManager>();

        GameObject player = new GameObject("PlayerRoot");
        player.transform.position = Vector3.zero;
        Rigidbody body = player.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        BoxCollider collider = player.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(0f, 0.8f, -0.5f);
        collider.size = new Vector3(1.0f, 1.7f, 1.2f);
        PlayerController playerController = player.AddComponent<PlayerController>();
        CrowdManager crowd = player.AddComponent<CrowdManager>();
        PlayerCombatController combat = player.AddComponent<PlayerCombatController>();
        Transform crowdAnchor = new GameObject("CrowdAnchor").transform;
        crowdAnchor.SetParent(player.transform, false);
        Transform aimOrigin = new GameObject("AimOrigin").transform;
        aimOrigin.SetParent(player.transform, false);
        aimOrigin.localPosition = new Vector3(0f, 1f, 0.2f);
        TextMesh countLabel = CreateWorldText("CrowdCountLabel", player.transform, "10", new Vector3(0f, 2.15f, -0.65f), 0.17f, Color.white);
        countLabel.gameObject.AddComponent<Billboard>();

        Camera camera = CreateCamera(tuning.cameraOffset, Quaternion.Euler(tuning.cameraEuler));
        camera.name = "Main Camera";
        camera.tag = "MainCamera";
        GameObject cameraRig = new GameObject("CameraRig");
        camera.transform.SetParent(cameraRig.transform, true);
        CameraFollowRig follow = cameraRig.AddComponent<CameraFollowRig>();
        follow.Configure(tuning, player.transform);

        Canvas canvas = CreateCanvas("RuntimeCanvas");
        GameObject safe = CreateSafeArea(canvas.transform);
        GameplayUI gameplayUI = canvas.gameObject.AddComponent<GameplayUI>();
        Text levelText = CreateUIText("LevelText", safe.transform, "Level 1", 36, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.965f), new Vector2(360f, 60f));
        Text coinText = CreateUIText("CoinText", safe.transform, "0", 34, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.78f, 0.12f), new Vector2(0.84f, 0.965f), new Vector2(220f, 60f));
        Slider progress = CreateProgressBar("ProgressBar", safe.transform, new Vector2(0.5f, 0.925f), new Vector2(520f, 26f));
        GameObject bossPanel = CreatePanel("BossPanel", safe.transform, new Vector2(0.5f, 0.875f), new Vector2(660f, 74f), new Color(0.24f, 0.03f, 0.05f, 0.86f));
        Text bossText = CreateUIText("BossText", bossPanel.transform, "TANK BOSS", 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.68f), new Vector2(560f, 32f));
        Slider bossSlider = CreateProgressBar("BossHealth", bossPanel.transform, new Vector2(0.5f, 0.28f), new Vector2(560f, 22f));
        bossPanel.SetActive(false);
        GameObject prompt = CreatePanel("StartPrompt", safe.transform, new Vector2(0.5f, 0.36f), new Vector2(530f, 98f), new Color(0.05f, 0.13f, 0.24f, 0.82f));
        Text promptText = CreateUIText("PromptText", prompt.transform, "DRAG TO START", 38, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(480f, 80f));
        GameObject victoryPanel = CreateResultPanel("VictoryPanel", safe.transform, "VICTORY", out Text victoryCoins, out Button nextButton);
        GameplayUI capturedGameplayUI = gameplayUI;
        UnityEventTools.AddPersistentListener(nextButton.onClick, capturedGameplayUI.NextLevel);
        GameObject defeatPanel = CreateResultPanel("DefeatPanel", safe.transform, "DEFEAT", out Text defeatText, out Button retryButton);
        UnityEventTools.AddPersistentListener(retryButton.onClick, capturedGameplayUI.Retry);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);

        SetObject(crowd, "_tuning", tuning);
        SetObject(crowd, "_soldierPrefab", prefabs.playerSoldier);
        SetObject(crowd, "_poolManager", pool);
        SetObject(crowd, "_anchor", crowdAnchor);
        SetObject(crowd, "_countLabel", countLabel);

        playerController.Configure(tuning, input, crowd, run);
        combat.Configure(tuning, crowd, run, pool, prefabs.projectile, aimOrigin);
        vfx.Configure(pool, prefabs.floatingText);

        levelManager.Configure(tuning, levels, pool, crowd, prefabs.trackSegment, prefabs.gate, prefabs.enemyGroup, prefabs.enemySoldier, prefabs.obstacle, prefabs.bossTank, prefabs.finishLine, levelRoot.transform);
        run.Configure(tuning, levelManager, crowd, gameplayUI);
        gameplayUI.Configure(playerController, levelManager);

        SetObject(gameplayUI, "_levelText", levelText);
        SetObject(gameplayUI, "_coinText", coinText);
        SetObject(gameplayUI, "_stateText", promptText);
        SetObject(gameplayUI, "_progressSlider", progress);
        SetObject(gameplayUI, "_bossPanel", bossPanel);
        SetObject(gameplayUI, "_bossSlider", bossSlider);
        SetObject(gameplayUI, "_bossText", bossText);
        SetObject(gameplayUI, "_startPrompt", prompt);
        SetObject(gameplayUI, "_victoryPanel", victoryPanel);
        SetObject(gameplayUI, "_victoryCoinsText", victoryCoins);
        SetObject(gameplayUI, "_defeatPanel", defeatPanel);
        SetObject(gameplayUI, "_defeatText", defeatText);
        SetObject(gameplayUI, "_player", playerController);
        SetObject(gameplayUI, "_levelManager", levelManager);

        EditorSceneManager.SaveScene(scene, ScenePath + "/Game.unity");
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath + "/Boot.unity", true),
            new EditorBuildSettingsScene(ScenePath + "/MainMenu.unity", true),
            new EditorBuildSettingsScene(ScenePath + "/Game.unity", true)
        };
    }

    private static void ValidatePrefabFolder(List<string> failures)
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabPath });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                failures.Add("Could not load prefab: " + path);
                continue;
            }

            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
            if (missing > 0)
            {
                failures.Add($"{path} has {missing} missing scripts.");
            }
        }
    }

    private static void ValidateScene(string path, List<string> failures, System.Action<Scene, List<string>> validate)
    {
        if (!System.IO.File.Exists(path))
        {
            failures.Add("Missing scene: " + path);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            if (missing > 0)
            {
                failures.Add($"{path}/{root.name} has {missing} missing scripts.");
            }
        }
        validate(scene, failures);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static void ValidateBootScene(Scene scene, List<string> failures)
    {
        if (Object.FindAnyObjectByType<GameBootstrapper>() == null)
        {
            failures.Add("Boot scene is missing GameBootstrapper.");
        }
    }

    private static void ValidateMainMenuScene(Scene scene, List<string> failures)
    {
        if (Object.FindAnyObjectByType<MainMenuUI>() == null)
        {
            failures.Add("MainMenu scene is missing MainMenuUI.");
        }

        if (Object.FindAnyObjectByType<Canvas>() == null)
        {
            failures.Add("MainMenu scene is missing a Canvas.");
        }
    }

    private static void ValidateGameScene(Scene scene, List<string> failures)
    {
        UpgradeDefinition[] upgrades = AssetDatabase.FindAssets("t:UpgradeDefinition", new[] { UpgradeDataPath })
            .Select(guid => AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => asset != null)
            .ToArray();
        GameBootstrapper.EnsureServices(upgrades);

        LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>();
        CrowdManager crowd = Object.FindAnyObjectByType<CrowdManager>();
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        GameplayUI ui = Object.FindAnyObjectByType<GameplayUI>();
        PoolManager pool = Object.FindAnyObjectByType<PoolManager>();

        if (levelManager == null)
        {
            failures.Add("Game scene is missing LevelManager.");
            return;
        }
        if (crowd == null)
        {
            failures.Add("Game scene is missing CrowdManager.");
        }
        if (player == null)
        {
            failures.Add("Game scene is missing PlayerController.");
        }
        if (ui == null)
        {
            failures.Add("Game scene is missing GameplayUI.");
        }
        if (Object.FindAnyObjectByType<VfxManager>() == null)
        {
            failures.Add("Game scene is missing VfxManager.");
        }
        if (pool == null)
        {
            failures.Add("Game scene is missing PoolManager.");
        }

        levelManager.BuildCurrentLevel();

        if (levelManager.CurrentLevel == null)
        {
            failures.Add("LevelManager did not select a current level.");
        }
        if (crowd != null && crowd.Count <= 0)
        {
            failures.Add("CrowdManager did not spawn a positive starting count.");
        }
        if (Object.FindObjectsByType<GateController>(FindObjectsInactive.Exclude).Length == 0)
        {
            failures.Add("Game scene validation spawned no gates.");
        }
        if (Object.FindObjectsByType<EnemyGroup>(FindObjectsInactive.Exclude).Length == 0)
        {
            failures.Add("Game scene validation spawned no enemy groups.");
        }
        if (Object.FindObjectsByType<ObstacleController>(FindObjectsInactive.Exclude).Length == 0)
        {
            failures.Add("Game scene validation spawned no obstacles.");
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/Bosses/PF_Boss_Tank.prefab") == null)
        {
            failures.Add("Missing tank boss prefab.");
        }
        if (Object.FindAnyObjectByType<FinishLineTrigger>() == null)
        {
            failures.Add("Game scene validation spawned no finish trigger.");
        }
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "ArmyRush";
        PlayerSettings.productName = "ArmyRush";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.accelerometerFrequency = 60;
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.armyrush.game");
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.18f;
        light.shadows = LightShadows.None;
        lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.72f, 0.82f, 0.94f);
    }

    private static Camera CreateCamera(Vector3 position, Quaternion rotation)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.transform.SetPositionAndRotation(position, rotation);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.42f, 0.78f, 1f);
        camera.fieldOfView = 52f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 450f;
        AudioListener listener = cameraObject.AddComponent<AudioListener>();
        _ = listener;
        return camera;
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        return canvas;
    }

    private static GameObject CreateSafeArea(Transform parent)
    {
        GameObject safe = new GameObject("SafeArea");
        RectTransform rect = safe.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        safe.AddComponent<SafeAreaFitter>();
        return safe;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static Text CreateUIText(string name, Transform parent, string text, int size, FontStyle style, TextAnchor anchor, Color color, Vector2 anchorPosition, Vector2 dimensions)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = Vector2.zero;
        Text label = obj.AddComponent<Text>();
        label.font = GetBuiltinFont();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = anchor;
        label.color = color;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = Mathf.Max(12, Mathf.RoundToInt(size * 0.55f));
        label.resizeTextMaxSize = size;
        return label;
    }

    private static Button CreateButton(string name, Transform parent, string text, Vector2 anchorPosition, Vector2 size, Color color)
    {
        GameObject obj = CreatePanel(name, parent, anchorPosition, size, color);
        Image image = obj.GetComponent<Image>();
        Button button = obj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.08f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.12f);
        colors.disabledColor = new Color(0.35f, 0.38f, 0.44f, 0.75f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.targetGraphic = image;
        obj.AddComponent<SimpleButtonAnimator>();

        if (!string.IsNullOrEmpty(text))
        {
            CreateUIText("Text", obj.transform, text, 52, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), size * 0.9f);
        }

        return button;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorPosition, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        Image image = obj.AddComponent<Image>();
        image.color = color;
        return obj;
    }

    private static Slider CreateProgressBar(string name, Transform parent, Vector2 anchorPosition, Vector2 size)
    {
        GameObject root = CreatePanel(name, parent, anchorPosition, size, new Color(0.04f, 0.09f, 0.16f, 0.72f));
        Slider slider = root.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;

        GameObject fill = CreatePanel("Fill", root.transform, new Vector2(0f, 0.5f), size, new Color(0.1f, 0.86f, 0.36f, 1f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        slider.fillRect = fillRect;
        return slider;
    }

    private static GameObject CreateResultPanel(string name, Transform parent, string header, out Text coinsText, out Button actionButton)
    {
        GameObject panel = CreatePanel(name, parent, new Vector2(0.5f, 0.48f), new Vector2(720f, 520f), new Color(0.04f, 0.11f, 0.22f, 0.94f));
        CreateUIText("Header", panel.transform, header, 68, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.78f), new Vector2(620f, 100f));
        coinsText = CreateUIText("Coins", panel.transform, "+0 COINS", 44, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.12f), new Vector2(0.5f, 0.55f), new Vector2(620f, 80f));
        actionButton = CreateButton("ActionButton", panel.transform, header == "VICTORY" ? "NEXT" : "RETRY", new Vector2(0.5f, 0.22f), new Vector2(430f, 105f), new Color(0.05f, 0.75f, 0.35f));
        return panel;
    }

    private static TextMesh CreateWorldText(string name, Transform parent, string text, Vector3 localPosition, float characterSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        TextMesh label = obj.AddComponent<TextMesh>();
        label.text = text;
        label.fontSize = 96;
        label.characterSize = characterSize;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = color;
        return label;
    }

    private static Transform AddMeshPart(Transform parent, string name, Mesh mesh, Material material, Vector3 localPosition, Vector3 localScale)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        MeshFilter filter = part.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = part.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return part.transform;
    }

    private static Material CreateMaterial(string name, Shader shader, Color color, float alpha)
    {
        string path = $"{MaterialPath}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.name = name;
        material.color = alpha > 0f ? new Color(color.r, color.g, color.b, 1f - alpha) : color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Mesh CreateMesh(string name, Mesh mesh)
    {
        string path = $"{MeshPath}/{name}.asset";
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            return existing;
        }
        mesh.name = name;
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Mesh BuildBoxMesh()
    {
        Vector3[] vertices =
        {
            new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,0.5f,-0.5f), new Vector3(-0.5f,0.5f,-0.5f),
            new Vector3(-0.5f,-0.5f,0.5f), new Vector3(0.5f,-0.5f,0.5f), new Vector3(0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f)
        };
        int[] triangles =
        {
            0,2,1, 0,3,2, 4,5,6, 4,6,7, 0,1,5, 0,5,4,
            2,3,7, 2,7,6, 0,4,7, 0,7,3, 1,2,6, 1,6,5
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh BuildWedgeMesh()
    {
        Vector3[] vertices =
        {
            new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,0.5f), new Vector3(-0.5f,-0.5f,0.5f),
            new Vector3(-0.42f,0.5f,-0.3f), new Vector3(0.42f,0.5f,-0.3f), new Vector3(0.42f,0.25f,0.45f), new Vector3(-0.42f,0.25f,0.45f)
        };
        int[] triangles =
        {
            0,1,2, 0,2,3, 4,6,5, 4,7,6, 0,4,5, 0,5,1,
            3,2,6, 3,6,7, 0,3,7, 0,7,4, 1,5,6, 1,6,2
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh BuildCylinderMesh(int sides)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        vertices.Add(new Vector3(0f, 0.5f, 0f));
        vertices.Add(new Vector3(0f, -0.5f, 0f));
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.PI * 2f * i / sides;
            float x = Mathf.Cos(angle) * 0.5f;
            float z = Mathf.Sin(angle) * 0.5f;
            vertices.Add(new Vector3(x, 0.5f, z));
            vertices.Add(new Vector3(x, -0.5f, z));
        }
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            int topA = 2 + i * 2;
            int bottomA = topA + 1;
            int topB = 2 + next * 2;
            int bottomB = topB + 1;
            triangles.AddRange(new[] { 0, topA, topB, 1, bottomB, bottomA, topA, bottomA, bottomB, topA, bottomB, topB });
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        EditorUtility.SetDirty(prefab);
        return prefab;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static UpgradeDefinition CreateUpgrade(UpgradeType type, string displayName, string description, int baseCost, float growth, float baseValue, float valuePerLevel)
    {
        string path = $"{UpgradeDataPath}/SO_Upgrade_{type}.asset";
        UpgradeDefinition definition = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<UpgradeDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }
        definition.type = type;
        definition.displayName = displayName;
        definition.description = description;
        definition.baseCost = baseCost;
        definition.costGrowth = growth;
        definition.baseValue = baseValue;
        definition.valuePerLevel = valuePerLevel;
        definition.maxLevel = 100;
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        return font;
    }

    private static void SetObject(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetBool(Object target, string propertyName, bool value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetObjectArray<T>(Object target, string propertyName, T[] values) where T : Object
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private sealed class MaterialSet
    {
        public Material playerBlue;
        public Material playerNavy;
        public Material enemyRed;
        public Material enemyCrimson;
        public Material skin;
        public Material track;
        public Material rail;
        public Material ocean;
        public Material gatePositive;
        public Material gateNegative;
        public Material projectile;
        public Material coin;
        public Material obstacle;
        public Material obstacleMetal;
        public Material uiBlue;
    }

    private sealed class MeshSet
    {
        public Mesh box;
        public Mesh panel;
        public Mesh cylinder;
        public Mesh road;
        public Mesh wedge;
    }

    private sealed class PrefabSet
    {
        public GameObject playerSoldier;
        public GameObject enemySoldier;
        public GameObject projectile;
        public GameObject trackSegment;
        public GameObject gate;
        public GameObject enemyGroup;
        public GameObject obstacle;
        public GameObject bossTank;
        public GameObject finishLine;
        public GameObject floatingText;
    }
}
