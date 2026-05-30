using System.Collections.Generic;
using System.Linq;
using ArmyRush;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
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
    private const string UiGeneratedPath = Root + "/Art/UI/Generated";
    private const string AppIconPath = UiGeneratedPath + "/APP_ArmyRush_1024.png";
    private const string LevelDataPath = Root + "/ScriptableObjects/Levels";
    private const string UpgradeDataPath = Root + "/ScriptableObjects/Upgrades";
    private const string TuningPath = Root + "/ScriptableObjects/Tuning";
    private const string BossDataPath = Root + "/ScriptableObjects/Bosses";
    private const string ObstacleDataPath = Root + "/ScriptableObjects/Obstacles";

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
        ObstacleDefinition[] obstacles = CreateObstacleDefinitions(prefabs);
        BossDefinition[] bosses = CreateBossDefinitions(prefabs);
        LevelData[] levels = CreateLevels(bosses, obstacles);

        CreateBootScene(upgrades);
        CreateMainMenuScene(upgrades, materials, meshes);
        CreateGameScene(tuning, upgrades, prefabs, levels, materials, meshes);
        PolishUiArt();
        GenerateAppIconAsset();
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
        ValidatePoolingSetup(failures);
        ValidateBossPrefabs(failures);
        ValidateObstaclePrefabs(failures);
        ValidateLevelDataAssets(failures);
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

    [MenuItem("ArmyRush/Build iOS Development Export")]
    public static void BuildIOSDevelopmentExport()
    {
        BuildIOSDevelopmentExport(iOSSdkVersion.DeviceSDK, "ArmyRush_iOSBuild", "iOS device");
    }

    [MenuItem("ArmyRush/Build iOS Simulator Development Export")]
    public static void BuildIOSSimulatorDevelopmentExport()
    {
        BuildIOSDevelopmentExport(iOSSdkVersion.SimulatorSDK, "ArmyRush_iOSSimulatorBuild", "iOS simulator");
    }

    private static void BuildIOSDevelopmentExport(iOSSdkVersion sdkVersion, string outputPath, string label)
    {
        try
        {
            ConfigureBuildSettings();
            ConfigurePlayerSettings(sdkVersion);

            string[] scenes =
            {
                ScenePath + "/Boot.unity",
                ScenePath + "/MainMenu.unity",
                ScenePath + "/Game.unity"
            };

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.Exception($"{label} development export failed: {report.summary.result}");
            }

            Debug.Log($"ArmyRush {label} development export succeeded: {report.summary.outputPath}");
        }
        finally
        {
            ConfigurePlayerSettings(iOSSdkVersion.DeviceSDK);
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("ArmyRush/Polish Character Prefabs")]
    public static void PolishCharacterPrefabs()
    {
        MaterialSet materials = LoadMaterialSet();
        MeshSet meshes = LoadMeshSet();
        ApplyCharacterPolish(PrefabPath + "/Player/PF_SoldierUnit_Blue.prefab", true, materials, meshes);
        ApplyCharacterPolish(PrefabPath + "/Enemies/PF_EnemyUnit_Red.prefab", false, materials, meshes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ArmyRush character prefab polish applied.");
    }

    [MenuItem("ArmyRush/Polish Game Environment")]
    public static void PolishGameEnvironment()
    {
        MaterialSet materials = LoadMaterialSet();
        MeshSet meshes = LoadMeshSet();
        Scene scene = EditorSceneManager.OpenScene(ScenePath + "/Game.unity", OpenSceneMode.Single);
        GameObject environment = GameObject.Find("EnvironmentRoot");
        if (environment == null)
        {
            environment = new GameObject("EnvironmentRoot");
        }

        BuildEnvironmentSet(environment.transform, materials, meshes);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ArmyRush game environment polish applied.");
    }

    [MenuItem("ArmyRush/Polish UI Art")]
    public static void PolishUiArt()
    {
        UiSpriteSet sprites = CreateUiSprites();
        MaterialSet materials = LoadMaterialSet();
        MeshSet meshes = LoadMeshSet();

        Scene mainMenu = EditorSceneManager.OpenScene(ScenePath + "/MainMenu.unity", OpenSceneMode.Single);
        ApplyUiArtToOpenScene(sprites);
        BuildMainMenuShowcase(materials, meshes);
        EditorSceneManager.MarkSceneDirty(mainMenu);
        EditorSceneManager.SaveScene(mainMenu);

        Scene game = EditorSceneManager.OpenScene(ScenePath + "/Game.unity", OpenSceneMode.Single);
        ApplyUiArtToOpenScene(sprites);
        EditorSceneManager.MarkSceneDirty(game);
        EditorSceneManager.SaveScene(game);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ArmyRush UI art polish applied.");
    }

    [MenuItem("ArmyRush/Generate App Icon")]
    public static void GenerateAppIconAsset()
    {
        Texture2D icon = CreateAppIconTexture();
        SaveAppIconTexture(icon);
        ConfigureAppIcons();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ArmyRush app icon generated and assigned.");
    }

    [MenuItem("ArmyRush/Repair UI Input Modules")]
    public static void RepairUiInputModules()
    {
        string[] scenePaths =
        {
            ScenePath + "/MainMenu.unity",
            ScenePath + "/Game.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EnsureEventSystem();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ArmyRush UI input modules repaired for Input System runtime.");
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
            UiGeneratedPath,
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
            BossDataPath,
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

        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
        {
            particleShader = shader;
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
            uiBlue = CreateMaterial("MAT_UIBlue", shader, new Color(0.1f, 0.32f, 1f), 0f),
            vfxParticle = CreateMaterial("MAT_VFXParticle", particleShader, Color.white, 0f)
        };
    }

    private static MaterialSet LoadMaterialSet()
    {
        return new MaterialSet
        {
            playerBlue = LoadRequiredAsset<Material>(MaterialPath + "/MAT_PlayerBlue.mat"),
            playerNavy = LoadRequiredAsset<Material>(MaterialPath + "/MAT_PlayerNavy.mat"),
            enemyRed = LoadRequiredAsset<Material>(MaterialPath + "/MAT_EnemyRed.mat"),
            enemyCrimson = LoadRequiredAsset<Material>(MaterialPath + "/MAT_EnemyCrimson.mat"),
            skin = LoadRequiredAsset<Material>(MaterialPath + "/MAT_StylizedSkin.mat"),
            track = LoadRequiredAsset<Material>(MaterialPath + "/MAT_TrackGray.mat"),
            rail = LoadRequiredAsset<Material>(MaterialPath + "/MAT_RailWhite.mat"),
            ocean = LoadRequiredAsset<Material>(MaterialPath + "/MAT_OceanBlue.mat"),
            gatePositive = LoadRequiredAsset<Material>(MaterialPath + "/MAT_GatePositive.mat"),
            gateNegative = LoadRequiredAsset<Material>(MaterialPath + "/MAT_GateNegative.mat"),
            projectile = LoadRequiredAsset<Material>(MaterialPath + "/MAT_ProjectileYellow.mat"),
            coin = LoadRequiredAsset<Material>(MaterialPath + "/MAT_CoinGold.mat"),
            obstacle = LoadRequiredAsset<Material>(MaterialPath + "/MAT_ObstacleWood.mat"),
            obstacleMetal = LoadRequiredAsset<Material>(MaterialPath + "/MAT_ObstacleMetal.mat"),
            uiBlue = LoadRequiredAsset<Material>(MaterialPath + "/MAT_UIBlue.mat"),
            vfxParticle = LoadRequiredAsset<Material>(MaterialPath + "/MAT_VFXParticle.mat")
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

    private static MeshSet LoadMeshSet()
    {
        return new MeshSet
        {
            box = LoadRequiredAsset<Mesh>(MeshPath + "/MSH_Box.asset"),
            panel = LoadRequiredAsset<Mesh>(MeshPath + "/MSH_GatePanel.asset"),
            cylinder = LoadRequiredAsset<Mesh>(MeshPath + "/MSH_Cylinder12.asset"),
            road = LoadRequiredAsset<Mesh>(MeshPath + "/MSH_RoadSegment.asset"),
            wedge = LoadRequiredAsset<Mesh>(MeshPath + "/MSH_Wedge.asset")
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
            CreateUpgrade(UpgradeType.BossDamage, "Boss Damage", "Increase damage dealt to boss targets.", 300, 1.18f, 1f, 0.08f),
            CreateUpgrade(UpgradeType.ObstacleDamage, "Obstacle Damage", "Increase damage dealt to barricades and breakables.", 220, 1.16f, 1f, 0.06f),
            CreateUpgrade(UpgradeType.CriticalChance, "Critical Chance", "Unlock a chance for high-impact critical volleys.", 400, 1.2f, 0f, 0.01f),
            CreateUpgrade(UpgradeType.CriticalDamage, "Critical Damage", "Increase the multiplier on critical volleys.", 500, 1.2f, 1.5f, 0.04f)
        };
    }

    private static ObstacleDefinition[] CreateObstacleDefinitions(PrefabSet prefabs)
    {
        return new[]
        {
            CreateObstacleDefinition("SO_Obstacle_Barricade", "Barricade", ObstacleKind.Barricade, prefabs.obstacle, 96, 26, 7, 12, 1, 1.65f, false),
            CreateObstacleDefinition("SO_Obstacle_CrateStack", "Crate Stack", ObstacleKind.CrateStack, prefabs.obstacleCrateStack, 112, 28, 8, 14, 1, 1.7f, false),
            CreateObstacleDefinition("SO_Obstacle_BarrelCluster", "Barrel Cluster", ObstacleKind.BarrelCluster, prefabs.obstacleBarrelCluster, 132, 34, 11, 20, 2, 1.75f, true),
            CreateObstacleDefinition("SO_Obstacle_ConcreteBlock", "Concrete Block", ObstacleKind.ConcreteBlock, prefabs.obstacleConcreteBlock, 170, 42, 12, 18, 1, 1.9f, false),
            CreateObstacleDefinition("SO_Obstacle_MilitaryTruck", "Military Truck", ObstacleKind.MilitaryTruck, prefabs.obstacleMilitaryTruck, 240, 54, 16, 28, 2, 2.25f, false),
            CreateObstacleDefinition("SO_Obstacle_Turret", "Sentry Turret", ObstacleKind.Turret, prefabs.obstacleTurret, 210, 48, 14, 26, 2, 1.8f, false),
            CreateObstacleDefinition("SO_Obstacle_FuelTank", "Fuel Tank", ObstacleKind.FuelTank, prefabs.obstacleFuelTank, 190, 45, 15, 32, 3, 2.0f, true)
        };
    }

    private static BossDefinition[] CreateBossDefinitions(PrefabSet prefabs)
    {
        BossDefinition tank = CreateBossDefinition(
            "SO_Boss_Tank",
            "TANK BOSS",
            prefabs.bossTank,
            BossAttackPattern.CannonVolley,
            900,
            72,
            22,
            260,
            20,
            18f,
            2.35f,
            0.72f,
            1.45f,
            3.2f,
            7,
            1,
            "CANNON",
            new Color(1f, 0.28f, 0.1f));

        BossDefinition helicopter = CreateBossDefinition(
            "SO_Boss_Helicopter",
            "HELI BOSS",
            prefabs.bossHelicopter,
            BossAttackPattern.MissileStrike,
            780,
            68,
            18,
            310,
            22,
            20f,
            1.9f,
            0.64f,
            1.1f,
            4.0f,
            6,
            1,
            "MISSILES",
            new Color(1f, 0.72f, 0.12f));

        BossDefinition mech = CreateBossDefinition(
            "SO_Boss_Mech",
            "MECH BOSS",
            prefabs.bossMech,
            BossAttackPattern.ShockwaveSlam,
            1120,
            92,
            26,
            370,
            26,
            17f,
            2.75f,
            0.82f,
            1.75f,
            3.4f,
            9,
            1,
            "SHOCKWAVE",
            new Color(1f, 0.34f, 0.1f));

        return new[] { tank, helicopter, mech };
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
        prefabs.obstacleCrateStack = CreateObstacleCrateStackPrefab(materials, meshes);
        prefabs.obstacleBarrelCluster = CreateObstacleBarrelClusterPrefab(materials, meshes);
        prefabs.obstacleConcreteBlock = CreateObstacleConcreteBlockPrefab(materials, meshes);
        prefabs.obstacleMilitaryTruck = CreateObstacleMilitaryTruckPrefab(materials, meshes);
        prefabs.obstacleTurret = CreateObstacleTurretPrefab(materials, meshes);
        prefabs.obstacleFuelTank = CreateObstacleFuelTankPrefab(materials, meshes);
        prefabs.obstacleVariants = new[]
        {
            prefabs.obstacle,
            prefabs.obstacleCrateStack,
            prefabs.obstacleBarrelCluster,
            prefabs.obstacleConcreteBlock,
            prefabs.obstacleMilitaryTruck,
            prefabs.obstacleTurret,
            prefabs.obstacleFuelTank
        };
        prefabs.bossTank = CreateBossTankPrefab(materials, meshes);
        prefabs.bossHelicopter = CreateBossHelicopterPrefab(materials, meshes);
        prefabs.bossMech = CreateBossMechPrefab(materials, meshes);
        prefabs.finishLine = CreateFinishLinePrefab(materials, meshes);
        prefabs.bonusCrate = CreateBonusCratePrefab(materials, meshes);
        prefabs.bonusEnd = CreateBonusEndPrefab(materials, meshes);
        prefabs.floatingText = CreateFloatingTextPrefab();
        prefabs.muzzleFlash = CreateParticleVfxPrefab("PF_VFX_MuzzleFlash", new Color(1f, 0.88f, 0.16f), 0.18f, 0.12f, 1.8f, 10, 0.12f, 0.08f, materials.vfxParticle);
        prefabs.hitSpark = CreateParticleVfxPrefab("PF_VFX_HitSpark", new Color(1f, 0.84f, 0.18f), 0.38f, 0.28f, 3.4f, 14, 0.14f, 0.18f, materials.vfxParticle);
        prefabs.gatePositiveBurst = CreateParticleVfxPrefab("PF_VFX_GatePositive", new Color(0.16f, 1f, 0.62f), 0.62f, 0.42f, 2.2f, 26, 0.22f, 0.55f, materials.vfxParticle);
        prefabs.gateNegativeBurst = CreateParticleVfxPrefab("PF_VFX_GateNegative", new Color(1f, 0.24f, 0.12f), 0.62f, 0.42f, 2.2f, 22, 0.2f, 0.55f, materials.vfxParticle);
        prefabs.crowdGainBurst = CreateParticleVfxPrefab("PF_VFX_CrowdGain", new Color(0.16f, 1f, 0.62f), 0.48f, 0.34f, 2.7f, 20, 0.15f, 0.42f, materials.vfxParticle);
        prefabs.crowdLossBurst = CreateParticleVfxPrefab("PF_VFX_CrowdLoss", new Color(1f, 0.24f, 0.12f), 0.48f, 0.34f, 2.7f, 18, 0.14f, 0.38f, materials.vfxParticle);
        prefabs.coinBurst = CreateParticleVfxPrefab("PF_VFX_CoinBurst", new Color(1f, 0.76f, 0.12f), 0.72f, 0.5f, 2.8f, 30, 0.17f, 0.75f, materials.vfxParticle);
        prefabs.obstacleDebris = CreateParticleVfxPrefab("PF_VFX_ObstacleDebris", new Color(0.72f, 0.38f, 0.16f), 0.64f, 0.46f, 3.2f, 26, 0.16f, 0.48f, materials.vfxParticle);
        prefabs.obstacleExplosion = CreateParticleVfxPrefab("PF_VFX_ObstacleExplosion", new Color(1f, 0.42f, 0.08f), 0.6f, 0.34f, 3.45f, 34, 0.28f, 0.82f, materials.vfxParticle);
        prefabs.victoryBurst = CreateParticleVfxPrefab("PF_VFX_VictoryBurst", new Color(0.22f, 1f, 0.48f), 1.05f, 0.72f, 3.7f, 44, 0.18f, 0.82f, materials.vfxParticle);
        prefabs.bossExplosion = CreateParticleVfxPrefab("PF_VFX_BossExplosion", new Color(1f, 0.34f, 0.08f), 1.1f, 0.72f, 4.2f, 46, 0.36f, 1.35f, materials.vfxParticle);
        prefabs.smokePuff = CreateSmokeVfxPrefab("PF_VFX_SmokePuff", new Color(0.36f, 0.39f, 0.42f, 0.56f), 1.05f, 0.72f, 0.68f, 12, 0.42f, 0.34f, materials.vfxParticle);
        prefabs.heavySmoke = CreateSmokeVfxPrefab("PF_VFX_HeavySmoke", new Color(0.25f, 0.26f, 0.28f, 0.62f), 1.42f, 1.05f, 0.52f, 20, 0.62f, 0.52f, materials.vfxParticle);
        return prefabs;
    }

    private static GameObject CreateSoldierPrefab(string name, bool player, MaterialSet materials, MeshSet meshes, string path)
    {
        GameObject root = new GameObject(name);
        PooledObject pooled = root.AddComponent<PooledObject>();
        SoldierUnitVisual visual = root.AddComponent<SoldierUnitVisual>();
        Transform bodyRoot = new GameObject("BodyRoot").transform;
        bodyRoot.SetParent(root.transform, false);

        Transform weapon = BuildSoldierVisualParts(bodyRoot, player, materials, meshes);

        SetObject(visual, "_bodyRoot", bodyRoot);
        SetObject(visual, "_weaponRoot", weapon);

        GameObject prefab = SavePrefab(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void ApplyCharacterPolish(string path, bool player, MaterialSet materials, MeshSet meshes)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            SoldierUnitVisual visual = root.GetComponent<SoldierUnitVisual>();
            if (visual == null)
            {
                visual = root.AddComponent<SoldierUnitVisual>();
            }

            Transform bodyRoot = root.transform.Find("BodyRoot");
            if (bodyRoot == null)
            {
                bodyRoot = new GameObject("BodyRoot").transform;
                bodyRoot.SetParent(root.transform, false);
            }

            Transform weapon = BuildSoldierVisualParts(bodyRoot, player, materials, meshes);
            SetObject(visual, "_bodyRoot", bodyRoot);
            SetObject(visual, "_weaponRoot", weapon);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform BuildSoldierVisualParts(Transform bodyRoot, bool player, MaterialSet materials, MeshSet meshes)
    {
        Material primary = player ? materials.playerBlue : materials.enemyRed;
        Material dark = player ? materials.playerNavy : materials.enemyCrimson;
        Material accent = player ? materials.rail : materials.obstacleMetal;

        UpsertMeshPart(bodyRoot, "Boots_L", meshes.box, dark, new Vector3(-0.12f, 0.12f, 0f), new Vector3(0.15f, 0.24f, 0.18f));
        UpsertMeshPart(bodyRoot, "Boots_R", meshes.box, dark, new Vector3(0.12f, 0.12f, 0f), new Vector3(0.15f, 0.24f, 0.18f));
        UpsertMeshPart(bodyRoot, "Leg_L", meshes.box, primary, new Vector3(-0.11f, 0.38f, 0f), new Vector3(0.12f, 0.32f, 0.14f));
        UpsertMeshPart(bodyRoot, "Leg_R", meshes.box, primary, new Vector3(0.11f, 0.38f, 0f), new Vector3(0.12f, 0.32f, 0.14f));
        UpsertMeshPart(bodyRoot, "Torso", meshes.box, primary, new Vector3(0f, 0.75f, 0f), new Vector3(0.44f, 0.48f, 0.26f));
        UpsertMeshPart(bodyRoot, "Vest", meshes.box, dark, new Vector3(0f, 0.8f, -0.04f), new Vector3(0.48f, 0.34f, 0.09f));
        UpsertMeshPart(bodyRoot, "VestAccent", meshes.box, accent, new Vector3(0f, 0.88f, 0.1f), new Vector3(0.34f, 0.07f, 0.04f));
        UpsertMeshPart(bodyRoot, "Belt", meshes.box, dark, new Vector3(0f, 0.55f, 0.01f), new Vector3(0.46f, 0.08f, 0.27f));
        UpsertMeshPart(bodyRoot, "Backpack", meshes.box, dark, new Vector3(0f, 0.79f, -0.19f), new Vector3(0.34f, 0.4f, 0.1f));
        UpsertMeshPart(bodyRoot, "Head", meshes.box, materials.skin, new Vector3(0f, 1.14f, 0f), new Vector3(0.3f, 0.3f, 0.27f));
        UpsertMeshPart(bodyRoot, "FaceVisor", meshes.box, dark, new Vector3(0f, 1.15f, 0.16f), new Vector3(0.2f, 0.06f, 0.03f));
        UpsertMeshPart(bodyRoot, "Helmet", meshes.wedge, dark, new Vector3(0f, 1.32f, 0f), new Vector3(0.36f, 0.17f, 0.31f));
        UpsertMeshPart(bodyRoot, "HelmetStripe", meshes.box, accent, new Vector3(0f, 1.39f, 0.03f), new Vector3(0.08f, 0.04f, 0.28f));
        UpsertMeshPart(bodyRoot, "HelmetBrim", meshes.box, dark, new Vector3(0f, 1.26f, 0.16f), new Vector3(0.38f, 0.05f, 0.09f));
        UpsertMeshPart(bodyRoot, "Arm_L", meshes.box, primary, new Vector3(-0.31f, 0.76f, -0.02f), new Vector3(0.11f, 0.38f, 0.12f));
        UpsertMeshPart(bodyRoot, "Arm_R", meshes.box, primary, new Vector3(0.31f, 0.76f, -0.02f), new Vector3(0.11f, 0.38f, 0.12f));
        UpsertMeshPart(bodyRoot, "Shoulder_L", meshes.box, dark, new Vector3(-0.31f, 0.98f, 0f), new Vector3(0.17f, 0.1f, 0.16f));
        UpsertMeshPart(bodyRoot, "Shoulder_R", meshes.box, dark, new Vector3(0.31f, 0.98f, 0f), new Vector3(0.17f, 0.1f, 0.16f));
        UpsertMeshPart(bodyRoot, "Glove_L", meshes.box, dark, new Vector3(-0.31f, 0.54f, 0.04f), new Vector3(0.12f, 0.09f, 0.13f));
        UpsertMeshPart(bodyRoot, "Glove_R", meshes.box, dark, new Vector3(0.31f, 0.54f, 0.04f), new Vector3(0.12f, 0.09f, 0.13f));

        Transform weaponRoot = UpsertEmptyTransform(bodyRoot, "WeaponRoot", new Vector3(0f, 0.83f, 0.22f), Vector3.zero, Vector3.one);
        Transform legacyRifle = bodyRoot.Find("Rifle");
        if (legacyRifle != null && legacyRifle.parent != weaponRoot)
        {
            legacyRifle.SetParent(weaponRoot, false);
        }

        UpsertMeshPart(weaponRoot, "Rifle", meshes.box, dark, Vector3.zero, new Vector3(0.13f, 0.12f, 0.46f));
        UpsertMeshPart(weaponRoot, "RifleStock", meshes.box, dark, new Vector3(0f, 0f, -0.28f), new Vector3(0.19f, 0.15f, 0.16f));
        UpsertMeshPart(weaponRoot, "RifleBarrel", meshes.box, materials.obstacleMetal, new Vector3(0f, 0f, 0.31f), new Vector3(0.07f, 0.07f, 0.26f));
        UpsertMeshPart(weaponRoot, "RifleMuzzle", meshes.box, accent, new Vector3(0f, 0f, 0.48f), new Vector3(0.09f, 0.09f, 0.05f));

        if (!player)
        {
            UpsertMeshPart(bodyRoot, "EnemyHelmetCrest", meshes.wedge, materials.enemyCrimson, new Vector3(0f, 1.46f, 0f), new Vector3(0.16f, 0.16f, 0.34f));
            UpsertMeshPart(bodyRoot, "EnemyShoulderProfile_L", meshes.box, materials.enemyCrimson, new Vector3(-0.43f, 1.02f, 0f), new Vector3(0.12f, 0.16f, 0.2f));
            UpsertMeshPart(bodyRoot, "EnemyShoulderProfile_R", meshes.box, materials.enemyCrimson, new Vector3(0.43f, 1.02f, 0f), new Vector3(0.12f, 0.16f, 0.2f));
        }

        return weaponRoot;
    }

    private static GameObject CreateProjectilePrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_ProjectileTracer");
        root.AddComponent<PooledObject>();
        root.AddComponent<Projectile>();
        AddMeshPart(root.transform, "Tracer", meshes.box, materials.projectile, Vector3.zero, new Vector3(0.08f, 0.08f, 0.55f));
        ConfigureProjectileTrail(root.AddComponent<TrailRenderer>(), materials.projectile);
        GameObject prefab = SavePrefab(root, PrefabPath + "/VFX/PF_ProjectileTracer.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void ConfigureProjectileTrail(TrailRenderer trail, Material material)
    {
        trail.time = 0.09f;
        trail.minVertexDistance = 0.035f;
        trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.11f), new Keyframe(1f, 0f));
        trail.startColor = new Color(1f, 0.88f, 0.18f, 0.78f);
        trail.endColor = new Color(1f, 0.38f, 0.08f, 0f);
        trail.alignment = LineAlignment.View;
        trail.numCapVertices = 2;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.emitting = false;
        trail.material = material;
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

    private static void BuildEnvironmentSet(Transform environment, MaterialSet materials, MeshSet meshes)
    {
        UpsertMeshPart(environment, "OceanPlane", meshes.box, materials.ocean, new Vector3(0f, -0.24f, 105f), new Vector3(32f, 0.08f, 250f));

        for (int i = 0; i < 9; i++)
        {
            float z = 8f + i * 23f;
            UpsertMeshPart(environment, $"Breakwater_L_{i:00}", meshes.box, materials.obstacleMetal, new Vector3(-5.25f, -0.02f, z), new Vector3(0.62f, 0.28f, 8.4f));
            UpsertMeshPart(environment, $"Breakwater_R_{i:00}", meshes.box, materials.obstacleMetal, new Vector3(5.25f, -0.02f, z), new Vector3(0.62f, 0.28f, 8.4f));
            UpsertMeshPart(environment, $"WaterHighlight_L_{i:00}", meshes.box, materials.rail, new Vector3(-11.2f, -0.18f, z + 3.8f), new Vector3(2.6f, 0.012f, 0.1f));
            UpsertMeshPart(environment, $"WaterHighlight_R_{i:00}", meshes.box, materials.rail, new Vector3(11.2f, -0.18f, z + 1.2f), new Vector3(2.6f, 0.012f, 0.1f));
        }

        for (int i = 0; i < 4; i++)
        {
            float z = 28f + i * 46f;
            AddHarborPad(environment, meshes, materials, -1f, z, i);
            AddHarborPad(environment, meshes, materials, 1f, z + 16f, i);
        }
    }

    private static void AddHarborPad(Transform environment, MeshSet meshes, MaterialSet materials, float side, float z, int index)
    {
        string prefix = side < 0f ? "Left" : "Right";
        float x = side * 9.1f;
        UpsertMeshPart(environment, $"{prefix}CoastPad_{index:00}", meshes.box, materials.track, new Vector3(x, -0.08f, z), new Vector3(4.4f, 0.18f, 18f));
        UpsertMeshPart(environment, $"{prefix}CoastEdge_{index:00}", meshes.box, materials.rail, new Vector3(side * 6.8f, 0.05f, z), new Vector3(0.18f, 0.3f, 18.4f));
        UpsertMeshPart(environment, $"{prefix}Container_A_{index:00}", meshes.box, materials.obstacle, new Vector3(x - side * 0.7f, 0.34f, z - 4.6f), new Vector3(1.45f, 0.62f, 1.15f));
        UpsertMeshPart(environment, $"{prefix}Container_B_{index:00}", meshes.box, materials.obstacleMetal, new Vector3(x + side * 0.85f, 0.29f, z + 1.8f), new Vector3(1.25f, 0.52f, 1.05f));
        UpsertMeshPart(environment, $"{prefix}BeaconPost_{index:00}", meshes.box, materials.rail, new Vector3(side * 6.15f, 1.05f, z + 6.2f), new Vector3(0.16f, 2.1f, 0.16f));
        UpsertMeshPart(environment, $"{prefix}BeaconLight_{index:00}", meshes.box, materials.coin, new Vector3(side * 6.15f, 2.18f, z + 6.2f), new Vector3(0.42f, 0.22f, 0.42f));
    }

    private static void BuildMainMenuShowcase(MaterialSet materials, MeshSet meshes)
    {
        GameObject existing = GameObject.Find("MenuShowcaseRoot");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject root = new GameObject("MenuShowcaseRoot");
        root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        UpsertMeshPart(root.transform, "OceanBackdrop", meshes.box, materials.ocean, new Vector3(0f, -0.58f, 8.6f), new Vector3(14f, 0.08f, 12f));
        UpsertMeshPart(root.transform, "Runway", meshes.road, materials.track, new Vector3(0f, -0.46f, 6.35f), new Vector3(3.0f, 0.12f, 3.6f));
        UpsertMeshPart(root.transform, "RunwayRail_L", meshes.box, materials.rail, new Vector3(-1.72f, -0.25f, 6.35f), new Vector3(0.1f, 0.28f, 3.7f));
        UpsertMeshPart(root.transform, "RunwayRail_R", meshes.box, materials.rail, new Vector3(1.72f, -0.25f, 6.35f), new Vector3(0.1f, 0.28f, 3.7f));
        UpsertMeshPart(root.transform, "RunwayStripe", meshes.box, materials.rail, new Vector3(0f, -0.38f, 6.35f), new Vector3(0.06f, 0.018f, 2.8f));
        UpsertMeshPart(root.transform, "HarborBlock_L", meshes.box, materials.obstacleMetal, new Vector3(-3.05f, -0.36f, 6.4f), new Vector3(0.72f, 0.2f, 2.8f));
        UpsertMeshPart(root.transform, "HarborBlock_R", meshes.box, materials.obstacleMetal, new Vector3(3.05f, -0.36f, 6.4f), new Vector3(0.72f, 0.2f, 2.8f));

        Transform banner = UpsertMeshPart(root.transform, "HeroGateBanner", meshes.box, materials.gatePositive, new Vector3(0f, 0.58f, 8.8f), new Vector3(2.9f, 0.22f, 0.07f));
        UpsertMeshPart(root.transform, "HeroGatePost_L", meshes.box, materials.rail, new Vector3(-1.56f, -0.1f, 8.8f), new Vector3(0.12f, 1.05f, 0.12f));
        UpsertMeshPart(root.transform, "HeroGatePost_R", meshes.box, materials.rail, new Vector3(1.56f, -0.1f, 8.8f), new Vector3(0.12f, 1.05f, 0.12f));
        UpsertMeshPart(root.transform, "HeroGateGlow", meshes.box, materials.projectile, new Vector3(0f, 0.25f, 8.74f), new Vector3(2.45f, 0.05f, 0.035f));

        Transform coin = UpsertMeshPart(root.transform, "HeroCoinMedal", meshes.cylinder, materials.coin, new Vector3(-1.35f, 0.34f, 4.9f), new Vector3(0.24f, 0.05f, 0.24f));
        coin.localRotation = Quaternion.Euler(74f, 0f, 0f);

        Vector3[] soldierPositions =
        {
            new Vector3(0f, -0.28f, 5.55f),
            new Vector3(-0.42f, -0.28f, 6.05f),
            new Vector3(0.42f, -0.28f, 6.05f),
            new Vector3(-0.82f, -0.28f, 6.55f),
            new Vector3(0.82f, -0.28f, 6.55f)
        };

        List<Transform> soldiers = new List<Transform>(soldierPositions.Length);
        for (int i = 0; i < soldierPositions.Length; i++)
        {
            Transform soldier = UpsertEmptyTransform(root.transform, $"HeroSoldier_{i:00}", soldierPositions[i], new Vector3(0f, 180f, 0f), Vector3.one * (i == 0 ? 0.62f : 0.54f));
            Transform bodyRoot = UpsertEmptyTransform(soldier, "BodyRoot", Vector3.zero, Vector3.zero, Vector3.one);
            BuildSoldierVisualParts(bodyRoot, true, materials, meshes);
            soldiers.Add(soldier);
        }

        MenuShowcaseAnimator animator = root.AddComponent<MenuShowcaseAnimator>();
        SetObjectArray(animator, "_soldiers", soldiers.ToArray());
        SetObject(animator, "_coin", coin);
        SetObject(animator, "_banner", banner);
    }

    private static UiSpriteSet CreateUiSprites()
    {
        EnsureFolder(UiGeneratedPath);
        return new UiSpriteSet
        {
            buttonFrame = CreateRoundedRectSprite("SPR_UI_ButtonFrame", 96, 18, new Vector4(20f, 20f, 20f, 20f)),
            panelFrame = CreateRoundedRectSprite("SPR_UI_PanelFrame", 96, 14, new Vector4(18f, 18f, 18f, 18f)),
            coinIcon = CreateIconSprite("SPR_UI_CoinIcon", "coin"),
            settingsIcon = CreateIconSprite("SPR_UI_SettingsIcon", "settings"),
            playIcon = CreateIconSprite("SPR_UI_PlayIcon", "play"),
            upgradeIcon = CreateIconSprite("SPR_UI_UpgradeIcon", "upgrade")
        };
    }

    private static void ApplyUiArtToOpenScene(UiSpriteSet sprites)
    {
        foreach (Image image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include))
        {
            if (image == null || image.name.Contains("DefeatFade"))
            {
                continue;
            }

            bool isButton = image.GetComponent<Button>() != null;
            image.sprite = isButton ? sprites.buttonFrame : sprites.panelFrame;
            image.type = Image.Type.Sliced;
            image.raycastTarget = isButton;
        }

        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            Transform safe = FindDeepChild(canvas.transform, "SafeArea");
            if (safe == null)
            {
                continue;
            }

            if (FindDeepChild(safe, "CoinsText") != null)
            {
                UpsertUiImage(safe, "CoinIcon", sprites.coinIcon, new Vector2(0.665f, 0.955f), new Vector2(54f, 54f), Color.white);
            }

            if (FindDeepChild(safe, "CoinText") != null)
            {
                UpsertUiImage(safe, "CoinIcon", sprites.coinIcon, new Vector2(0.685f, 0.965f), new Vector2(50f, 50f), Color.white);
            }
        }

        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include))
        {
            if (button == null)
            {
                continue;
            }

            if (button.name.Contains("Settings"))
            {
                UpsertUiImage(button.transform, "SettingsIcon", sprites.settingsIcon, new Vector2(0.18f, 0.5f), new Vector2(34f, 34f), Color.white);
                PlaceChildText(button.transform, "Text", new Vector2(0.62f, 0.5f), new Vector2(150f, 52f));
            }
            else if (button.name == "PlayButton")
            {
                UpsertUiImage(button.transform, "PlayIcon", sprites.playIcon, new Vector2(0.23f, 0.5f), new Vector2(58f, 58f), Color.white);
                PlaceChildText(button.transform, "Text", new Vector2(0.58f, 0.5f), new Vector2(270f, 86f));
            }
            else if (button.name.StartsWith("Upgrade_"))
            {
                UpsertUiImage(button.transform, "UpgradeIcon", sprites.upgradeIcon, new Vector2(0.17f, 0.52f), new Vector2(42f, 42f), Color.white);
                PlaceUpgradeButtonContent(button.transform);
            }
        }

        AddPanelAccent("SettingsPanel", sprites.panelFrame, new Color(0.12f, 0.42f, 1f, 0.9f));
        AddPanelAccent("VictoryPanel", sprites.panelFrame, new Color(0.14f, 0.9f, 0.42f, 0.95f));
        AddPanelAccent("DefeatPanel", sprites.panelFrame, new Color(1f, 0.24f, 0.12f, 0.95f));
        AddPanelAccent("BossPanel", sprites.panelFrame, new Color(1f, 0.24f, 0.12f, 0.9f));
    }

    private static Sprite CreateRoundedRectSprite(string name, int size, int radius, Vector4 border)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = Color.clear;
        Color fill = Color.white;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, IsInsideRoundedRect(x, y, size, radius) ? fill : clear);
            }
        }

        texture.Apply();
        return SaveSpriteTexture(name, texture, border);
    }

    private static Sprite CreateIconSprite(string name, string kind)
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
                float ny = (y + 0.5f - size * 0.5f) / (size * 0.5f);
                texture.SetPixel(x, y, GetIconPixel(kind, nx, ny));
            }
        }

        texture.Apply();
        return SaveSpriteTexture(name, texture, Vector4.zero);
    }

    private static Color GetIconPixel(string kind, float x, float y)
    {
        float radius = Mathf.Sqrt(x * x + y * y);
        if (kind == "coin")
        {
            if (radius > 0.88f)
            {
                return Color.clear;
            }

            if (Mathf.Abs(radius - 0.62f) < 0.07f || (x > -0.18f && x < 0.18f && y > -0.48f && y < 0.48f))
            {
                return new Color(1f, 0.9f, 0.22f, 1f);
            }

            return new Color(1f, 0.64f, 0.04f, 1f);
        }

        if (kind == "settings")
        {
            float angle = Mathf.Atan2(y, x);
            float teeth = Mathf.Sin(angle * 8f) > 0.35f ? 0.12f : 0f;
            if (radius > 0.32f && radius < 0.7f + teeth)
            {
                return Color.white;
            }

            return Color.clear;
        }

        if (kind == "play")
        {
            bool inside = x > -0.48f && x < 0.34f && Mathf.Abs(y) < (0.45f - x) * 0.62f;
            return inside ? Color.white : Color.clear;
        }

        if (kind == "upgrade")
        {
            bool shaft = Mathf.Abs(x) < 0.16f && y > -0.54f && y < 0.24f;
            bool head = y > 0.02f && y < 0.58f && Mathf.Abs(x) < 0.58f - y * 0.62f;
            return shaft || head ? Color.white : Color.clear;
        }

        return Color.clear;
    }

    private static bool IsInsideRoundedRect(int x, int y, int size, int radius)
    {
        int min = radius;
        int max = size - radius - 1;
        int cx = Mathf.Clamp(x, min, max);
        int cy = Mathf.Clamp(y, min, max);
        int dx = x - cx;
        int dy = y - cy;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static Sprite SaveSpriteTexture(string name, Texture2D texture, Vector4 border)
    {
        string path = UiGeneratedPath + "/" + name + ".png";
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        return LoadRequiredAsset<Sprite>(path);
    }

    private static Texture2D CreateAppIconTexture()
    {
        const int size = 1024;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color skyTop = new Color(0.08f, 0.72f, 1f, 1f);
        Color skyBottom = new Color(0.02f, 0.2f, 0.52f, 1f);
        Color navy = new Color(0.02f, 0.08f, 0.2f, 1f);
        Color deepBlue = new Color(0.02f, 0.22f, 0.58f, 1f);
        Color gold = new Color(1f, 0.7f, 0.06f, 1f);
        Color highlight = new Color(0.92f, 0.98f, 1f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
                float ny = (y + 0.5f - size * 0.5f) / (size * 0.5f);
                float vertical = Mathf.InverseLerp(-1f, 1f, ny);
                Color pixel = Color.Lerp(skyBottom, skyTop, vertical);

                float roadWidth = Mathf.Lerp(0.7f, 0.25f, Mathf.InverseLerp(-1f, 0.78f, ny));
                if (ny < 0.78f && Mathf.Abs(nx) < roadWidth)
                {
                    pixel = Color.Lerp(new Color(0.03f, 0.09f, 0.18f, 1f), deepBlue, vertical * 0.35f);
                    if (Mathf.Abs(nx) < 0.035f && ny < 0.62f && Mathf.Sin((ny + 1f) * 32f) > -0.15f)
                    {
                        pixel = gold;
                    }
                }

                float shield = Mathf.Sqrt((nx * nx) / 0.62f + ((ny - 0.1f) * (ny - 0.1f)) / 0.82f);
                if (shield < 0.72f)
                {
                    pixel = Color.Lerp(navy, deepBlue, Mathf.Clamp01((ny + 0.55f) * 0.9f));
                }

                bool helmet = Mathf.Pow(nx / 0.42f, 2f) + Mathf.Pow((ny - 0.28f) / 0.28f, 2f) < 1f && ny > 0.12f;
                bool visor = Mathf.Abs(nx) < 0.32f && ny > 0.12f && ny < 0.25f;
                bool body = Mathf.Pow(nx / 0.48f, 2f) + Mathf.Pow((ny + 0.2f) / 0.42f, 2f) < 1f && ny < 0.08f;
                if (body)
                {
                    pixel = new Color(0.04f, 0.42f, 0.95f, 1f);
                }

                if (helmet)
                {
                    pixel = highlight;
                }

                if (visor)
                {
                    pixel = navy;
                }

                bool chevron = ny < -0.38f && ny > -0.66f && Mathf.Abs(Mathf.Abs(nx) - (ny + 0.82f) * 0.78f) < 0.08f;
                if (chevron)
                {
                    pixel = gold;
                }

                bool rim = shield > 0.68f && shield < 0.73f;
                if (rim)
                {
                    pixel = gold;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return texture;
    }

    private static void SaveAppIconTexture(Texture2D texture)
    {
        EnsureFolder(UiGeneratedPath);
        System.IO.File.WriteAllBytes(AppIconPath, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(AppIconPath);
        TextureImporter importer = AssetImporter.GetAtPath(AppIconPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static Texture2D EnsureAppIconTexture()
    {
        if (!System.IO.File.Exists(AppIconPath))
        {
            SaveAppIconTexture(CreateAppIconTexture());
        }

        return LoadRequiredAsset<Texture2D>(AppIconPath);
    }

    private static void ConfigureAppIcons()
    {
        Texture2D icon = EnsureAppIconTexture();
        foreach (IconKind iconKind in System.Enum.GetValues(typeof(IconKind)))
        {
            int[] iconSizes = PlayerSettings.GetIconSizes(NamedBuildTarget.iOS, iconKind);
            if (iconSizes == null || iconSizes.Length == 0)
            {
                continue;
            }

            Texture2D[] icons = Enumerable.Repeat(icon, iconSizes.Length).ToArray();
            PlayerSettings.SetIcons(NamedBuildTarget.iOS, icons, iconKind);
        }
    }

    private static Image UpsertUiImage(Transform parent, string name, Sprite sprite, Vector2 anchorPosition, Vector2 size, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject obj = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void PlaceChildText(Transform parent, string name, Vector2 anchorPosition, Vector2 size)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            return;
        }

        RectTransform rect = child.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
    }

    private static void PlaceUpgradeButtonContent(Transform buttonTransform)
    {
        PlaceChildText(buttonTransform, "Title", new Vector2(0.58f, 0.66f), new Vector2(245f, 32f));
        PlaceChildText(buttonTransform, "Level", new Vector2(0.43f, 0.29f), new Vector2(140f, 26f));
        PlaceChildText(buttonTransform, "Cost", new Vector2(0.82f, 0.29f), new Vector2(118f, 30f));
        ConfigureMenuText(buttonTransform, "Title", 21, TextAnchor.MiddleLeft, Color.white);
        ConfigureMenuText(buttonTransform, "Level", 18, TextAnchor.MiddleLeft, Color.white);
        ConfigureMenuText(buttonTransform, "Cost", 22, TextAnchor.MiddleRight, new Color(1f, 0.83f, 0.2f));
    }

    private static void ConfigureMenuText(Transform parent, string name, int fontSize, TextAnchor alignment, Color color)
    {
        Transform child = parent.Find(name);
        if (child == null || !child.TryGetComponent(out Text text))
        {
            return;
        }

        text.fontSize = fontSize;
        text.resizeTextMaxSize = fontSize;
        text.resizeTextMinSize = Mathf.Max(11, Mathf.RoundToInt(fontSize * 0.58f));
        text.alignment = alignment;
        text.color = color;
    }

    private static void AddPanelAccent(string panelName, Sprite sprite, Color color)
    {
        Transform panel = FindDeepChildInOpenScene(panelName);
        if (panel == null)
        {
            return;
        }

        Image stripe = UpsertUiImage(panel, "AccentStripe", sprite, new Vector2(0.5f, 0.98f), new Vector2(0f, 8f), color);
        RectTransform rect = stripe.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 0.98f);
        rect.anchorMax = new Vector2(0.92f, 0.98f);
        rect.sizeDelta = new Vector2(0f, 8f);
    }

    private static Transform FindDeepChildInOpenScene(string name)
    {
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            Transform child = FindDeepChild(canvas.transform, name);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindDeepChild(parent.GetChild(i), name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
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
        GameObject root = CreateObstacleRoot("PF_Obstacle_Barricade", new Vector3(2f, 1.5f, 1.1f), new Vector3(0f, 0.75f, 0f), out Damageable damageable, out ObstacleController obstacle);
        AddMeshPart(root.transform, "Crate_L", meshes.box, materials.obstacle, new Vector3(-0.48f, 0.38f, 0f), new Vector3(0.78f, 0.72f, 0.72f));
        AddMeshPart(root.transform, "Crate_R", meshes.box, materials.obstacle, new Vector3(0.48f, 0.38f, 0f), new Vector3(0.78f, 0.72f, 0.72f));
        AddMeshPart(root.transform, "MetalBand", meshes.box, materials.obstacleMetal, new Vector3(0f, 0.82f, -0.02f), new Vector3(1.85f, 0.16f, 0.82f));
        AddMeshPart(root.transform, "HazardStripe", meshes.box, materials.projectile, new Vector3(0f, 1.0f, -0.48f), new Vector3(1.7f, 0.08f, 0.08f));
        return FinalizeObstaclePrefab(root, damageable, obstacle, "100", new Vector3(0f, 1.55f, 0f), PrefabPath + "/Obstacles/PF_Obstacle_Barricade.prefab");
    }

    private static GameObject CreateObstacleCrateStackPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = CreateObstacleRoot("PF_Obstacle_CrateStack", new Vector3(2.2f, 1.7f, 1.25f), new Vector3(0f, 0.85f, 0f), out Damageable damageable, out ObstacleController obstacle);
        AddMeshPart(root.transform, "BottomCrate_L", meshes.box, materials.obstacle, new Vector3(-0.52f, 0.35f, 0f), new Vector3(0.82f, 0.68f, 0.82f));
        AddMeshPart(root.transform, "BottomCrate_R", meshes.box, materials.obstacle, new Vector3(0.52f, 0.35f, 0f), new Vector3(0.82f, 0.68f, 0.82f));
        AddMeshPart(root.transform, "TopCrate", meshes.box, materials.obstacle, new Vector3(0f, 0.98f, -0.08f), new Vector3(0.92f, 0.62f, 0.78f));
        AddMeshPart(root.transform, "CrossBand_A", meshes.box, materials.obstacleMetal, new Vector3(0f, 0.72f, -0.48f), new Vector3(1.96f, 0.09f, 0.08f));
        AddMeshPart(root.transform, "CrossBand_B", meshes.box, materials.obstacleMetal, new Vector3(0f, 1.3f, -0.48f), new Vector3(1.24f, 0.09f, 0.08f));
        return FinalizeObstaclePrefab(root, damageable, obstacle, "120", new Vector3(0f, 1.78f, 0f), PrefabPath + "/Obstacles/PF_Obstacle_CrateStack.prefab");
    }

    private static GameObject CreateObstacleBarrelClusterPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = CreateObstacleRoot("PF_Obstacle_BarrelCluster", new Vector3(2.1f, 1.45f, 1.45f), new Vector3(0f, 0.72f, 0f), out Damageable damageable, out ObstacleController obstacle);
        AddMeshPart(root.transform, "Barrel_L", meshes.cylinder, materials.enemyRed, new Vector3(-0.52f, 0.62f, 0f), new Vector3(0.48f, 1.18f, 0.48f));
        AddMeshPart(root.transform, "Barrel_R", meshes.cylinder, materials.enemyRed, new Vector3(0.52f, 0.62f, 0f), new Vector3(0.48f, 1.18f, 0.48f));
        AddMeshPart(root.transform, "Barrel_Back", meshes.cylinder, materials.enemyCrimson, new Vector3(0f, 0.62f, 0.44f), new Vector3(0.46f, 1.12f, 0.46f));
        AddMeshPart(root.transform, "WarningBand", meshes.box, materials.projectile, new Vector3(0f, 0.86f, -0.5f), new Vector3(1.55f, 0.11f, 0.08f));
        AddMeshPart(root.transform, "BasePallet", meshes.box, materials.obstacleMetal, new Vector3(0f, 0.12f, 0f), new Vector3(1.65f, 0.18f, 1.12f));
        return FinalizeObstaclePrefab(root, damageable, obstacle, "140", new Vector3(0f, 1.58f, 0f), PrefabPath + "/Obstacles/PF_Obstacle_BarrelCluster.prefab");
    }

    private static GameObject CreateObstacleConcreteBlockPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = CreateObstacleRoot("PF_Obstacle_ConcreteBlock", new Vector3(2.35f, 1.35f, 1.1f), new Vector3(0f, 0.68f, 0f), out Damageable damageable, out ObstacleController obstacle);
        AddMeshPart(root.transform, "ConcreteBody", meshes.box, materials.track, new Vector3(0f, 0.48f, 0f), new Vector3(2.05f, 0.92f, 0.86f));
        AddMeshPart(root.transform, "TopCap", meshes.box, materials.rail, new Vector3(0f, 1.0f, 0f), new Vector3(2.18f, 0.18f, 0.96f));
        AddMeshPart(root.transform, "Rebar_L", meshes.box, materials.obstacleMetal, new Vector3(-0.54f, 1.23f, 0f), new Vector3(0.08f, 0.48f, 0.08f));
        AddMeshPart(root.transform, "Rebar_R", meshes.box, materials.obstacleMetal, new Vector3(0.54f, 1.23f, 0f), new Vector3(0.08f, 0.48f, 0.08f));
        AddMeshPart(root.transform, "WarningStripe", meshes.box, materials.projectile, new Vector3(0f, 0.72f, -0.48f), new Vector3(1.7f, 0.1f, 0.08f));
        return FinalizeObstaclePrefab(root, damageable, obstacle, "180", new Vector3(0f, 1.72f, 0f), PrefabPath + "/Obstacles/PF_Obstacle_ConcreteBlock.prefab");
    }

    private static GameObject CreateObstacleMilitaryTruckPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = CreateObstacleRoot("PF_Obstacle_MilitaryTruck", new Vector3(2.7f, 1.7f, 2.6f), new Vector3(0f, 0.84f, 0f), out Damageable damageable, out ObstacleController obstacle);
        AddMeshPart(root.transform, "Chassis", meshes.box, materials.obstacleMetal, new Vector3(0f, 0.48f, 0.08f), new Vector3(2.18f, 0.38f, 2.05f));
        AddMeshPart(root.transform, "Cab", meshes.wedge, materials.enemyCrimson, new Vector3(0f, 1.02f, -0.74f), new Vector3(1.35f, 0.88f, 0.92f));
        AddMeshPart(root.transform, "CanvasCover", meshes.box, materials.enemyRed, new Vector3(0f, 1.04f, 0.46f), new Vector3(1.72f, 0.78f, 1.2f));
        Transform wheelFL = AddMeshPart(root.transform, "Wheel_FL", meshes.cylinder, materials.obstacleMetal, new Vector3(-1.15f, 0.32f, -0.72f), new Vector3(0.32f, 0.18f, 0.32f));
        Transform wheelFR = AddMeshPart(root.transform, "Wheel_FR", meshes.cylinder, materials.obstacleMetal, new Vector3(1.15f, 0.32f, -0.72f), new Vector3(0.32f, 0.18f, 0.32f));
        Transform wheelBL = AddMeshPart(root.transform, "Wheel_BL", meshes.cylinder, materials.obstacleMetal, new Vector3(-1.15f, 0.32f, 0.82f), new Vector3(0.32f, 0.18f, 0.32f));
        Transform wheelBR = AddMeshPart(root.transform, "Wheel_BR", meshes.cylinder, materials.obstacleMetal, new Vector3(1.15f, 0.32f, 0.82f), new Vector3(0.32f, 0.18f, 0.32f));
        wheelFL.localRotation = Quaternion.Euler(0f, 0f, 90f);
        wheelFR.localRotation = Quaternion.Euler(0f, 0f, 90f);
        wheelBL.localRotation = Quaternion.Euler(0f, 0f, 90f);
        wheelBR.localRotation = Quaternion.Euler(0f, 0f, 90f);
        AddMeshPart(root.transform, "Bumper", meshes.box, materials.projectile, new Vector3(0f, 0.64f, -1.22f), new Vector3(1.72f, 0.14f, 0.1f));
        return FinalizeObstaclePrefab(root, damageable, obstacle, "260", new Vector3(0f, 1.98f, 0f), PrefabPath + "/Obstacles/PF_Obstacle_MilitaryTruck.prefab");
    }

    private static GameObject CreateObstacleTurretPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = CreateObstacleRoot("PF_Obstacle_Turret", new Vector3(2.0f, 1.85f, 1.75f), new Vector3(0f, 0.92f, 0f), out Damageable damageable, out ObstacleController obstacle);
        AddMeshPart(root.transform, "Base", meshes.cylinder, materials.obstacleMetal, new Vector3(0f, 0.22f, 0f), new Vector3(1.22f, 0.36f, 1.22f));
        AddMeshPart(root.transform, "Column", meshes.cylinder, materials.rail, new Vector3(0f, 0.76f, 0f), new Vector3(0.64f, 0.96f, 0.64f));
        AddMeshPart(root.transform, "TurretHead", meshes.wedge, materials.enemyRed, new Vector3(0f, 1.35f, -0.05f), new Vector3(1.25f, 0.64f, 1.0f));
        AddMeshPart(root.transform, "Barrel", meshes.box, materials.obstacleMetal, new Vector3(0f, 1.32f, -0.92f), new Vector3(0.22f, 0.22f, 1.25f));
        AddMeshPart(root.transform, "OpticGlow", meshes.box, materials.projectile, new Vector3(0f, 1.46f, -0.58f), new Vector3(0.54f, 0.1f, 0.08f));
        return FinalizeObstaclePrefab(root, damageable, obstacle, "220", new Vector3(0f, 2.15f, 0f), PrefabPath + "/Obstacles/PF_Obstacle_Turret.prefab");
    }

    private static GameObject CreateObstacleFuelTankPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = CreateObstacleRoot("PF_Obstacle_FuelTank", new Vector3(2.45f, 1.45f, 1.45f), new Vector3(0f, 0.72f, 0f), out Damageable damageable, out ObstacleController obstacle);
        Transform tank = AddMeshPart(root.transform, "TankBody", meshes.cylinder, materials.enemyRed, new Vector3(0f, 0.82f, 0f), new Vector3(0.62f, 1.7f, 0.62f));
        tank.localRotation = Quaternion.Euler(0f, 0f, 90f);
        Transform capL = AddMeshPart(root.transform, "Cap_L", meshes.cylinder, materials.obstacleMetal, new Vector3(-0.92f, 0.82f, 0f), new Vector3(0.66f, 0.08f, 0.66f));
        Transform capR = AddMeshPart(root.transform, "Cap_R", meshes.cylinder, materials.obstacleMetal, new Vector3(0.92f, 0.82f, 0f), new Vector3(0.66f, 0.08f, 0.66f));
        capL.localRotation = Quaternion.Euler(0f, 0f, 90f);
        capR.localRotation = Quaternion.Euler(0f, 0f, 90f);
        AddMeshPart(root.transform, "Saddle_L", meshes.box, materials.obstacleMetal, new Vector3(-0.62f, 0.22f, 0f), new Vector3(0.16f, 0.36f, 0.88f));
        AddMeshPart(root.transform, "Saddle_R", meshes.box, materials.obstacleMetal, new Vector3(0.62f, 0.22f, 0f), new Vector3(0.16f, 0.36f, 0.88f));
        AddMeshPart(root.transform, "HazardBand", meshes.box, materials.projectile, new Vector3(0f, 1.08f, -0.62f), new Vector3(1.52f, 0.1f, 0.08f));
        return FinalizeObstaclePrefab(root, damageable, obstacle, "200", new Vector3(0f, 1.74f, 0f), PrefabPath + "/Obstacles/PF_Obstacle_FuelTank.prefab");
    }

    private static GameObject CreateObstacleRoot(string name, Vector3 colliderSize, Vector3 colliderCenter, out Damageable damageable, out ObstacleController obstacle)
    {
        GameObject root = new GameObject(name);
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = colliderSize;
        collider.center = colliderCenter;
        collider.isTrigger = true;
        damageable = root.AddComponent<Damageable>();
        obstacle = root.AddComponent<ObstacleController>();
        return root;
    }

    private static GameObject FinalizeObstaclePrefab(GameObject root, Damageable damageable, ObstacleController obstacle, string labelText, Vector3 labelPosition, string path)
    {
        TextMesh label = CreateWorldText("HealthLabel", root.transform, labelText, labelPosition, 0.13f, Color.white);
        label.gameObject.AddComponent<Billboard>();
        SetObject(damageable, "_label", label);
        SetObject(obstacle, "_damageable", damageable);
        SetObject(obstacle, "_healthLabel", label);
        GameObject prefab = SavePrefab(root, path);
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

    private static GameObject CreateBossHelicopterPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_Boss_Helicopter");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(3.6f, 2.2f, 3.4f);
        collider.center = new Vector3(0f, 1.25f, 0f);
        collider.isTrigger = true;
        Damageable damageable = root.AddComponent<Damageable>();
        BossController boss = root.AddComponent<BossController>();

        AddMeshPart(root.transform, "Shadow", meshes.cylinder, materials.obstacleMetal, new Vector3(0f, 0.04f, 0.2f), new Vector3(1.45f, 0.02f, 0.72f));
        AddMeshPart(root.transform, "Fuselage", meshes.box, materials.enemyRed, new Vector3(0f, 1.34f, 0f), new Vector3(1.75f, 0.62f, 1.45f));
        AddMeshPart(root.transform, "Cockpit", meshes.wedge, materials.enemyCrimson, new Vector3(0f, 1.42f, -0.82f), new Vector3(1.1f, 0.48f, 0.8f));
        AddMeshPart(root.transform, "TailBoom", meshes.box, materials.obstacleMetal, new Vector3(0f, 1.34f, 1.34f), new Vector3(0.34f, 0.28f, 1.9f));
        AddMeshPart(root.transform, "TailRotor", meshes.box, materials.rail, new Vector3(0f, 1.38f, 2.42f), new Vector3(0.08f, 0.95f, 0.08f));
        AddMeshPart(root.transform, "TailFin", meshes.wedge, materials.enemyCrimson, new Vector3(0f, 1.8f, 2.02f), new Vector3(0.12f, 0.78f, 0.62f));
        AddMeshPart(root.transform, "MainRotor_A", meshes.box, materials.rail, new Vector3(0f, 1.94f, 0f), new Vector3(4.2f, 0.05f, 0.14f));
        AddMeshPart(root.transform, "MainRotor_B", meshes.box, materials.rail, new Vector3(0f, 1.95f, 0f), new Vector3(0.14f, 0.05f, 4.2f));
        AddMeshPart(root.transform, "MissilePod_L", meshes.box, materials.obstacleMetal, new Vector3(-1.2f, 1.04f, -0.34f), new Vector3(0.28f, 0.3f, 0.9f));
        AddMeshPart(root.transform, "MissilePod_R", meshes.box, materials.obstacleMetal, new Vector3(1.2f, 1.04f, -0.34f), new Vector3(0.28f, 0.3f, 0.9f));
        AddMeshPart(root.transform, "MissileTip_L", meshes.cylinder, materials.projectile, new Vector3(-1.2f, 1.04f, -0.9f), new Vector3(0.18f, 0.18f, 0.18f));
        AddMeshPart(root.transform, "MissileTip_R", meshes.cylinder, materials.projectile, new Vector3(1.2f, 1.04f, -0.9f), new Vector3(0.18f, 0.18f, 0.18f));

        TextMesh label = CreateWorldText("BossHealthLabel", root.transform, "HELI BOSS\n1000", new Vector3(0f, 2.45f, 0f), 0.12f, Color.white);
        label.gameObject.AddComponent<Billboard>();
        SetObject(damageable, "_label", label);
        SetObject(boss, "_damageable", damageable);
        SetObject(boss, "_healthLabel", label);

        GameObject prefab = SavePrefab(root, PrefabPath + "/Bosses/PF_Boss_Helicopter.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateBossMechPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_Boss_Mech");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(3.2f, 3.0f, 2.6f);
        collider.center = new Vector3(0f, 1.45f, 0f);
        collider.isTrigger = true;
        Damageable damageable = root.AddComponent<Damageable>();
        BossController boss = root.AddComponent<BossController>();

        AddMeshPart(root.transform, "Foot_L", meshes.box, materials.obstacleMetal, new Vector3(-0.62f, 0.18f, -0.18f), new Vector3(0.72f, 0.28f, 1.1f));
        AddMeshPart(root.transform, "Foot_R", meshes.box, materials.obstacleMetal, new Vector3(0.62f, 0.18f, -0.18f), new Vector3(0.72f, 0.28f, 1.1f));
        AddMeshPart(root.transform, "Leg_L", meshes.box, materials.enemyCrimson, new Vector3(-0.62f, 0.72f, 0f), new Vector3(0.38f, 0.9f, 0.42f));
        AddMeshPart(root.transform, "Leg_R", meshes.box, materials.enemyCrimson, new Vector3(0.62f, 0.72f, 0f), new Vector3(0.38f, 0.9f, 0.42f));
        AddMeshPart(root.transform, "Pelvis", meshes.box, materials.obstacleMetal, new Vector3(0f, 1.18f, 0f), new Vector3(1.55f, 0.38f, 0.86f));
        AddMeshPart(root.transform, "Torso", meshes.wedge, materials.enemyRed, new Vector3(0f, 1.78f, 0f), new Vector3(1.85f, 1.05f, 1.2f));
        AddMeshPart(root.transform, "CoreGlow", meshes.box, materials.projectile, new Vector3(0f, 1.76f, -0.64f), new Vector3(0.64f, 0.18f, 0.08f));
        AddMeshPart(root.transform, "Head", meshes.box, materials.enemyCrimson, new Vector3(0f, 2.5f, -0.05f), new Vector3(0.92f, 0.48f, 0.72f));
        AddMeshPart(root.transform, "EyeGlow", meshes.box, materials.projectile, new Vector3(0f, 2.53f, -0.44f), new Vector3(0.62f, 0.1f, 0.08f));
        AddMeshPart(root.transform, "Arm_L", meshes.box, materials.obstacleMetal, new Vector3(-1.32f, 1.65f, -0.04f), new Vector3(0.38f, 1.1f, 0.42f));
        AddMeshPart(root.transform, "Arm_R", meshes.box, materials.obstacleMetal, new Vector3(1.32f, 1.65f, -0.04f), new Vector3(0.38f, 1.1f, 0.42f));
        AddMeshPart(root.transform, "Cannon_L", meshes.box, materials.rail, new Vector3(-1.32f, 1.12f, -0.48f), new Vector3(0.28f, 0.24f, 0.95f));
        AddMeshPart(root.transform, "Cannon_R", meshes.box, materials.rail, new Vector3(1.32f, 1.12f, -0.48f), new Vector3(0.28f, 0.24f, 0.95f));
        AddMeshPart(root.transform, "Shoulder_L", meshes.box, materials.enemyCrimson, new Vector3(-1.14f, 2.1f, 0.04f), new Vector3(0.62f, 0.42f, 0.82f));
        AddMeshPart(root.transform, "Shoulder_R", meshes.box, materials.enemyCrimson, new Vector3(1.14f, 2.1f, 0.04f), new Vector3(0.62f, 0.42f, 0.82f));

        TextMesh label = CreateWorldText("BossHealthLabel", root.transform, "MECH BOSS\n1000", new Vector3(0f, 3.25f, 0f), 0.12f, Color.white);
        label.gameObject.AddComponent<Billboard>();
        SetObject(damageable, "_label", label);
        SetObject(boss, "_damageable", damageable);
        SetObject(boss, "_healthLabel", label);

        GameObject prefab = SavePrefab(root, PrefabPath + "/Bosses/PF_Boss_Mech.prefab");
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

    private static GameObject CreateBonusCratePrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_BonusCrate");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.55f, 1.65f, 1.2f);
        collider.center = new Vector3(0f, 0.78f, 0f);
        collider.isTrigger = true;

        Damageable damageable = root.AddComponent<Damageable>();
        BonusCrateController bonus = root.AddComponent<BonusCrateController>();

        AddMeshPart(root.transform, "BaseCrate", meshes.box, materials.obstacle, new Vector3(0f, 0.4f, 0f), new Vector3(1.35f, 0.78f, 0.95f));
        AddMeshPart(root.transform, "TopCrate", meshes.box, materials.obstacle, new Vector3(0f, 1.05f, 0f), new Vector3(1.05f, 0.52f, 0.76f));
        AddMeshPart(root.transform, "GoldBand_Front", meshes.box, materials.coin, new Vector3(0f, 0.79f, -0.5f), new Vector3(1.48f, 0.14f, 0.08f));
        AddMeshPart(root.transform, "GoldBand_Back", meshes.box, materials.coin, new Vector3(0f, 0.79f, 0.5f), new Vector3(1.48f, 0.14f, 0.08f));
        AddMeshPart(root.transform, "MetalRim_L", meshes.box, materials.obstacleMetal, new Vector3(-0.74f, 0.73f, 0f), new Vector3(0.08f, 1.3f, 1.02f));
        AddMeshPart(root.transform, "MetalRim_R", meshes.box, materials.obstacleMetal, new Vector3(0.74f, 0.73f, 0f), new Vector3(0.08f, 1.3f, 1.02f));
        AddMeshPart(root.transform, "CoinIcon", meshes.cylinder, materials.coin, new Vector3(0f, 1.45f, -0.42f), new Vector3(0.38f, 0.08f, 0.38f));

        TextMesh healthLabel = CreateWorldText("HealthLabel", root.transform, "80", new Vector3(0f, 1.75f, 0f), 0.115f, Color.white);
        healthLabel.gameObject.AddComponent<Billboard>();
        TextMesh rewardLabel = CreateWorldText("RewardLabel", root.transform, "+25", new Vector3(0f, 2.05f, 0f), 0.13f, new Color(1f, 0.82f, 0.16f));
        rewardLabel.gameObject.AddComponent<Billboard>();

        SetObject(damageable, "_label", healthLabel);
        SetObject(bonus, "_damageable", damageable);
        SetObject(bonus, "_healthLabel", healthLabel);
        SetObject(bonus, "_rewardLabel", rewardLabel);

        GameObject prefab = SavePrefab(root, PrefabPath + "/Levels/PF_BonusCrate.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateBonusEndPrefab(MaterialSet materials, MeshSet meshes)
    {
        GameObject root = new GameObject("PF_BonusEnd");
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(7f, 3f, 0.7f);
        collider.center = new Vector3(0f, 1.2f, 0f);
        collider.isTrigger = true;
        root.AddComponent<BonusEndTrigger>();

        AddMeshPart(root.transform, "ClaimStrip", meshes.box, materials.coin, new Vector3(0f, 0.05f, 0f), new Vector3(7f, 0.08f, 0.65f));
        AddMeshPart(root.transform, "LeftPillar", meshes.box, materials.rail, new Vector3(-3.2f, 1.45f, 0f), new Vector3(0.18f, 2.9f, 0.18f));
        AddMeshPart(root.transform, "RightPillar", meshes.box, materials.rail, new Vector3(3.2f, 1.45f, 0f), new Vector3(0.18f, 2.9f, 0.18f));
        AddMeshPart(root.transform, "Banner", meshes.box, materials.coin, new Vector3(0f, 2.5f, 0f), new Vector3(6.25f, 0.58f, 0.09f));
        AddMeshPart(root.transform, "BannerTrim", meshes.box, materials.obstacleMetal, new Vector3(0f, 2.14f, 0f), new Vector3(6.45f, 0.12f, 0.12f));
        TextMesh label = CreateWorldText("ClaimLabel", root.transform, "CLAIM", new Vector3(0f, 2.51f, -0.08f), 0.14f, Color.white);
        label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        GameObject prefab = SavePrefab(root, PrefabPath + "/Levels/PF_BonusEnd.prefab");
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

    private static GameObject CreateParticleVfxPrefab(
        string name,
        Color color,
        float lifetime,
        float particleLifetime,
        float speed,
        int burstCount,
        float startSize,
        float radius,
        Material material)
    {
        GameObject root = new GameObject(name);
        root.AddComponent<PooledObject>();
        PooledParticleVfx pooledVfx = root.AddComponent<PooledParticleVfx>();
        ParticleSystem particles = root.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.duration = lifetime;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = speed;
        main.startSize = startSize;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.None;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(1, burstCount)) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius;
        shape.randomDirectionAmount = 0.45f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(Color.Lerp(color, Color.white, 0.35f), 0.45f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.82f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        SetObjectArray(pooledVfx, "_systems", new[] { particles });
        SetFloat(pooledVfx, "_lifetime", lifetime + 0.08f);

        GameObject prefab = SavePrefab(root, PrefabPath + "/VFX/" + name + ".prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateSmokeVfxPrefab(
        string name,
        Color color,
        float lifetime,
        float particleLifetime,
        float speed,
        int burstCount,
        float startSize,
        float radius,
        Material material)
    {
        GameObject root = new GameObject(name);
        root.AddComponent<PooledObject>();
        PooledParticleVfx pooledVfx = root.AddComponent<PooledParticleVfx>();
        ParticleSystem particles = root.AddComponent<ParticleSystem>();

        VfxManager.ConfigureSmokeParticleSystem(particles, color, lifetime, particleLifetime, speed, burstCount, startSize, radius, material);

        pooledVfx.Configure(new[] { particles }, lifetime + 0.12f);

        GameObject prefab = SavePrefab(root, PrefabPath + "/VFX/" + name + ".prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static LevelData[] CreateLevels(BossDefinition[] bosses, ObstacleDefinition[] obstacleDefinitions)
    {
        List<LevelData> levels = new List<LevelData>();
        BossDefinition fallbackBoss = bosses != null && bosses.Length > 0 ? bosses[0] : null;
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
            if (data.hasBoss)
            {
                data.trackLength = Mathf.Max(data.trackLength, 152f);
            }
            BossDefinition bossDefinition = data.hasBoss && bosses != null && bosses.Length > 0 ? bosses[((i / 5) - 1) % bosses.Length] : fallbackBoss;
            data.bossDefinition = data.hasBoss ? bossDefinition : null;
            data.bossHealth = data.hasBoss && bossDefinition != null ? bossDefinition.GetHealth(i, 0) : 0;
            data.bonusCrateCount = i < 4 ? 2 : 3;
            data.bonusCrateHealth = Mathf.RoundToInt(70f + i * 22f);
            data.bonusCrateReward = Mathf.RoundToInt(Mathf.Lerp(24f, 130f, (i - 1) / 19f));
            data.bonusSectionLength = 34f;
            data.gates.Clear();
            data.enemyGroups.Clear();
            data.obstacles.Clear();

            AddDesignedLevelData(data, i, obstacleDefinitions);
            EditorUtility.SetDirty(data);
            levels.Add(data);
        }

        return levels.ToArray();
    }

    private static void AddDesignedLevelData(LevelData data, int level, ObstacleDefinition[] obstacleDefinitions)
    {
        AddGatePair(data, 16f, GateOperation.Add, 5 + level, GateOperation.Add, 10 + level * 2);
        AddEnemy(data, 32f, 0f, 8 + level * 3, 8 + level);
        AddGatePair(data, 47f, GateOperation.Multiply, level < 6 ? 2 : 3, GateOperation.Add, 18 + level * 3);
        AddObstacle(data, 64f, level % 2 == 0 ? -1.4f : 1.4f, 70 + level * 35, 5 + level, SelectObstacleDefinition(obstacleDefinitions, level, 0));
        AddGatePair(data, 82f, level >= 3 ? GateOperation.Subtract : GateOperation.Add, level >= 3 ? 10 + level : 10, GateOperation.Multiply, 2);
        AddEnemy(data, 101f, level % 2 == 0 ? 1.2f : -1.2f, 16 + level * 5, 10 + level * 2);

        const float lateObstacleZ = 118f;
        float bossApproachStart = GetBossApproachStart(data);
        if (level >= 4 && lateObstacleZ < bossApproachStart - 8f)
        {
            AddObstacle(data, lateObstacleZ, -1.6f, 110 + level * 45, 8 + level, SelectObstacleDefinition(obstacleDefinitions, level, 1));
            AddObstacle(data, lateObstacleZ, 1.6f, 130 + level * 48, 8 + level, SelectObstacleDefinition(obstacleDefinitions, level, 2));
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

    private static ObstacleDefinition SelectObstacleDefinition(ObstacleDefinition[] definitions, int level, int salt)
    {
        if (definitions == null || definitions.Length == 0)
        {
            return null;
        }

        int index;
        if (level <= 2)
        {
            index = 1;
        }
        else if (level <= 4)
        {
            index = salt == 0 ? 0 : 1;
        }
        else
        {
            index = Mathf.Abs((level + salt * 2) % definitions.Length);
        }

        return definitions[Mathf.Clamp(index, 0, definitions.Length - 1)];
    }

    private static void AddObstacle(LevelData data, float z, float x, int health, int penalty, ObstacleDefinition definition)
    {
        data.obstacles.Add(new ObstacleSpawnData
        {
            definition = definition,
            z = z,
            x = x,
            health = health,
            collisionPenalty = penalty,
            coinReward = definition != null ? definition.GetCoinReward(data.levelIndex, 0) : 0,
            width = definition != null ? definition.width : 1.6f
        });
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

    private static void CreateMainMenuScene(UpgradeDefinition[] upgrades, MaterialSet materials, MeshSet meshes)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateLighting();
        Camera camera = CreateCamera(new Vector3(0f, 5.6f, -8f), Quaternion.Euler(32f, 0f, 0f));
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.38f, 0.78f, 1f);
        BuildMainMenuShowcase(materials, meshes);

        Canvas canvas = CreateCanvas("MainMenuCanvas");
        GameObject safe = CreateSafeArea(canvas.transform);
        MainMenuUI menu = canvas.gameObject.AddComponent<MainMenuUI>();
        SettingsPanelUI settings = canvas.gameObject.AddComponent<SettingsPanelUI>();

        Text title = CreateUIText("Title", safe.transform, "ARMY RUSH", 86, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.08f, 0.16f, 0.32f), new Vector2(0.5f, 0.88f), new Vector2(760f, 120f));
        Text coins = CreateUIText("CoinsText", safe.transform, "0", 42, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.78f, 0.12f), new Vector2(0.82f, 0.955f), new Vector2(260f, 80f));
        Text level = CreateUIText("LevelText", safe.transform, "LEVEL 1", 40, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.78f), new Vector2(400f, 80f));
        Button settingsButton = CreateButton("SettingsButton", safe.transform, "SETTINGS", new Vector2(0.16f, 0.955f), new Vector2(250f, 64f), new Color(0.05f, 0.13f, 0.24f));
        UnityEventTools.AddPersistentListener(settingsButton.onClick, settings.Open);

        Button play = CreateButton("PlayButton", safe.transform, "PLAY", new Vector2(0.5f, 0.13f), new Vector2(520f, 122f), new Color(0.05f, 0.78f, 0.35f));
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);

        UpgradeType[] types =
        {
            UpgradeType.Damage,
            UpgradeType.FireRate,
            UpgradeType.StartingTroops,
            UpgradeType.CoinReward,
            UpgradeType.BossDamage,
            UpgradeType.ObstacleDamage,
            UpgradeType.CriticalChance,
            UpgradeType.CriticalDamage
        };
        UpgradeButtonView[] views = new UpgradeButtonView[types.Length];
        for (int i = 0; i < views.Length; i++)
        {
            int row = i / 2;
            int column = i % 2;
            Vector2 anchor = new Vector2(column == 0 ? 0.29f : 0.71f, 0.565f - row * 0.096f);
            Button button = CreateButton("Upgrade_" + types[i], safe.transform, string.Empty, anchor, new Vector2(392f, 88f), new Color(0.08f, 0.28f, 0.95f));
            UpgradeButtonView view = button.gameObject.AddComponent<UpgradeButtonView>();
            Text titleText = CreateUIText("Title", button.transform, types[i].ToString().ToUpperInvariant(), 21, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.58f, 0.66f), new Vector2(245f, 32f));
            Text levelText = CreateUIText("Level", button.transform, "Lv. 0", 18, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.43f, 0.29f), new Vector2(140f, 26f));
            Text costText = CreateUIText("Cost", button.transform, "100", 22, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.83f, 0.2f), new Vector2(0.82f, 0.29f), new Vector2(118f, 30f));
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
        CreateSettingsPanel(safe.transform, settings);

        EditorSceneManager.SaveScene(scene, ScenePath + "/MainMenu.unity");
    }

    private static void CreateGameScene(GlobalTuning tuning, UpgradeDefinition[] upgrades, PrefabSet prefabs, LevelData[] levels, MaterialSet materials, MeshSet meshes)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateLighting();

        GameObject bootstrap = new GameObject("GameBootstrapper");
        GameBootstrapper bootstrapper = bootstrap.AddComponent<GameBootstrapper>();
        SetObjectArray(bootstrapper, "_upgradeDefinitions", upgrades);
        SetBool(bootstrapper, "_loadMainMenuOnStart", false);

        GameObject environment = new GameObject("EnvironmentRoot");
        BuildEnvironmentSet(environment.transform, materials, meshes);

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
        cameraRig.transform.SetPositionAndRotation(tuning.cameraOffset + Vector3.forward * tuning.cameraLookAhead, Quaternion.Euler(tuning.cameraEuler));
        camera.transform.SetParent(cameraRig.transform, false);
        camera.transform.localPosition = Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
        CameraFollowRig follow = cameraRig.AddComponent<CameraFollowRig>();
        follow.Configure(tuning, player.transform);

        Canvas canvas = CreateCanvas("RuntimeCanvas");
        GameObject safe = CreateSafeArea(canvas.transform);
        Image defeatFade = CreateFullscreenImage("DefeatFadeOverlay", canvas.transform, new Color(0.24f, 0.02f, 0.04f, 0f));
        defeatFade.transform.SetAsFirstSibling();
        defeatFade.gameObject.SetActive(false);
        GameplayUI gameplayUI = canvas.gameObject.AddComponent<GameplayUI>();
        SettingsPanelUI settings = canvas.gameObject.AddComponent<SettingsPanelUI>();
        Text levelText = CreateUIText("LevelText", safe.transform, "Level 1", 36, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.965f), new Vector2(360f, 60f));
        Text coinText = CreateUIText("CoinText", safe.transform, "0", 34, FontStyle.Bold, TextAnchor.MiddleRight, new Color(1f, 0.78f, 0.12f), new Vector2(0.84f, 0.965f), new Vector2(220f, 60f));
        Button settingsButton = CreateButton("SettingsButton", safe.transform, "SETTINGS", new Vector2(0.16f, 0.965f), new Vector2(230f, 58f), new Color(0.05f, 0.13f, 0.24f));
        UnityEventTools.AddPersistentListener(settingsButton.onClick, settings.Open);
        Slider progress = CreateProgressBar("ProgressBar", safe.transform, new Vector2(0.5f, 0.925f), new Vector2(520f, 26f));
        GameObject bossPanel = CreatePanel("BossPanel", safe.transform, new Vector2(0.5f, 0.875f), new Vector2(660f, 74f), new Color(0.24f, 0.03f, 0.05f, 0.86f));
        Text bossText = CreateUIText("BossText", bossPanel.transform, "TANK BOSS", 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.68f), new Vector2(560f, 32f));
        Slider bossSlider = CreateProgressBar("BossHealth", bossPanel.transform, new Vector2(0.5f, 0.28f), new Vector2(560f, 22f));
        bossPanel.SetActive(false);
        GameObject prompt = CreatePanel("StartPrompt", safe.transform, new Vector2(0.5f, 0.36f), new Vector2(530f, 98f), new Color(0.05f, 0.13f, 0.24f, 0.82f));
        Text promptText = CreateUIText("PromptText", prompt.transform, "DRAG TO START", 38, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(480f, 80f));
        GameObject victoryPanel = CreateResultPanel("VictoryPanel", safe.transform, "VICTORY", out Text victoryCoins, out Button nextButton, out Button victoryUpgradeButton, out Button victoryRewardedButton, out Text victoryStatusText);
        GameplayUI capturedGameplayUI = gameplayUI;
        UnityEventTools.AddPersistentListener(nextButton.onClick, capturedGameplayUI.NextLevel);
        GameObject defeatPanel = CreateResultPanel("DefeatPanel", safe.transform, "DEFEAT", out Text defeatText, out Button retryButton, out Button defeatUpgradeButton, out Button defeatReviveButton, out Text defeatStatusText);
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
        vfx.Configure(pool, prefabs.floatingText, prefabs.muzzleFlash, prefabs.hitSpark, prefabs.gatePositiveBurst, prefabs.gateNegativeBurst, prefabs.crowdGainBurst, prefabs.crowdLossBurst, prefabs.coinBurst, prefabs.obstacleDebris, prefabs.obstacleExplosion, prefabs.victoryBurst, prefabs.bossExplosion, prefabs.smokePuff, prefabs.heavySmoke);

        levelManager.Configure(tuning, levels, pool, crowd, prefabs.trackSegment, prefabs.gate, prefabs.enemyGroup, prefabs.enemySoldier, prefabs.obstacle, prefabs.bossTank, prefabs.finishLine, prefabs.bonusCrate, prefabs.bonusEnd, levelRoot.transform);
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
        SetObject(gameplayUI, "_victoryRewardedButton", victoryRewardedButton);
        SetObject(gameplayUI, "_victoryStatusText", victoryStatusText);
        SetObject(gameplayUI, "_victoryUpgradeButton", victoryUpgradeButton);
        SetObject(gameplayUI, "_defeatPanel", defeatPanel);
        SetObject(gameplayUI, "_defeatText", defeatText);
        SetObject(gameplayUI, "_defeatReviveButton", defeatReviveButton);
        SetObject(gameplayUI, "_defeatStatusText", defeatStatusText);
        SetObject(gameplayUI, "_defeatUpgradeButton", defeatUpgradeButton);
        SetObject(gameplayUI, "_defeatFadeImage", defeatFade);
        SetObject(gameplayUI, "_player", playerController);
        SetObject(gameplayUI, "_levelManager", levelManager);
        CreateSettingsPanel(safe.transform, settings);

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

    private static void ValidatePoolingSetup(List<string> failures)
    {
        ValidatePooledPrefab(failures, PrefabPath + "/Player/PF_SoldierUnit_Blue.prefab", typeof(SoldierUnitVisual));
        ValidatePooledPrefab(failures, PrefabPath + "/Enemies/PF_EnemyUnit_Red.prefab", typeof(SoldierUnitVisual));
        ValidatePooledPrefab(failures, PrefabPath + "/VFX/PF_ProjectileTracer.prefab", typeof(Projectile));
        ValidatePooledPrefab(failures, PrefabPath + "/VFX/PF_FloatingText.prefab", typeof(FloatingText));

        string[] particleVfxPrefabs =
        {
            PrefabPath + "/VFX/PF_VFX_MuzzleFlash.prefab",
            PrefabPath + "/VFX/PF_VFX_HitSpark.prefab",
            PrefabPath + "/VFX/PF_VFX_GatePositive.prefab",
            PrefabPath + "/VFX/PF_VFX_GateNegative.prefab",
            PrefabPath + "/VFX/PF_VFX_CrowdGain.prefab",
            PrefabPath + "/VFX/PF_VFX_CrowdLoss.prefab",
            PrefabPath + "/VFX/PF_VFX_CoinBurst.prefab",
            PrefabPath + "/VFX/PF_VFX_ObstacleDebris.prefab",
            PrefabPath + "/VFX/PF_VFX_ObstacleExplosion.prefab",
            PrefabPath + "/VFX/PF_VFX_VictoryBurst.prefab",
            PrefabPath + "/VFX/PF_VFX_BossExplosion.prefab",
            PrefabPath + "/VFX/PF_VFX_SmokePuff.prefab",
            PrefabPath + "/VFX/PF_VFX_HeavySmoke.prefab"
        };

        foreach (string path in particleVfxPrefabs)
        {
            ValidatePooledPrefab(failures, path, typeof(PooledParticleVfx));
        }
    }

    private static void ValidateBossPrefabs(List<string> failures)
    {
        ValidateBossPrefab(failures, PrefabPath + "/Bosses/PF_Boss_Tank.prefab", 6);
        ValidateBossPrefab(failures, PrefabPath + "/Bosses/PF_Boss_Helicopter.prefab", 10);
        ValidateBossPrefab(failures, PrefabPath + "/Bosses/PF_Boss_Mech.prefab", 12);
    }

    private static void ValidateBossPrefab(List<string> failures, string path, int minimumMeshRenderers)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            failures.Add("Missing boss prefab: " + path);
            return;
        }

        if (prefab.GetComponent<BossController>() == null)
        {
            failures.Add(path + " is missing BossController.");
        }
        if (prefab.GetComponent<Damageable>() == null)
        {
            failures.Add(path + " is missing Damageable.");
        }
        Collider collider = prefab.GetComponent<Collider>();
        if (collider == null || !collider.isTrigger)
        {
            failures.Add(path + " is missing a trigger collider.");
        }
        if (prefab.GetComponentsInChildren<MeshRenderer>(true).Length < minimumMeshRenderers)
        {
            failures.Add(path + " does not contain enough visual mesh parts.");
        }
        if (prefab.GetComponentInChildren<TextMesh>(true) == null)
        {
            failures.Add(path + " is missing a boss health label.");
        }
    }

    private static void ValidateObstaclePrefabs(List<string> failures)
    {
        ValidateObstaclePrefab(failures, PrefabPath + "/Obstacles/PF_Obstacle_Barricade.prefab", 4);
        ValidateObstaclePrefab(failures, PrefabPath + "/Obstacles/PF_Obstacle_CrateStack.prefab", 5);
        ValidateObstaclePrefab(failures, PrefabPath + "/Obstacles/PF_Obstacle_BarrelCluster.prefab", 5);
        ValidateObstaclePrefab(failures, PrefabPath + "/Obstacles/PF_Obstacle_ConcreteBlock.prefab", 5);
        ValidateObstaclePrefab(failures, PrefabPath + "/Obstacles/PF_Obstacle_MilitaryTruck.prefab", 8);
        ValidateObstaclePrefab(failures, PrefabPath + "/Obstacles/PF_Obstacle_Turret.prefab", 5);
        ValidateObstaclePrefab(failures, PrefabPath + "/Obstacles/PF_Obstacle_FuelTank.prefab", 6);

        ObstacleDefinition[] definitions = AssetDatabase.FindAssets("t:ObstacleDefinition", new[] { ObstacleDataPath })
            .Select(guid => AssetDatabase.LoadAssetAtPath<ObstacleDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => asset != null)
            .ToArray();
        if (definitions.Length < 7)
        {
            failures.Add("Expected at least seven obstacle definition assets.");
        }

        foreach (ObstacleDefinition definition in definitions)
        {
            if (definition.prefab == null)
            {
                failures.Add(definition.name + " is missing an obstacle prefab reference.");
            }
            if (definition.baseHealth <= 0 || definition.healthPerLevel < 0 || definition.collisionPenalty < 0 || definition.baseCoinReward < 0 || definition.width <= 0f)
            {
                failures.Add(definition.name + " has invalid obstacle balance values.");
            }
        }
    }

    private static void ValidateObstaclePrefab(List<string> failures, string path, int minimumMeshRenderers)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            failures.Add("Missing obstacle prefab: " + path);
            return;
        }

        if (prefab.GetComponent<ObstacleController>() == null)
        {
            failures.Add(path + " is missing ObstacleController.");
        }
        if (prefab.GetComponent<Damageable>() == null)
        {
            failures.Add(path + " is missing Damageable.");
        }
        Collider collider = prefab.GetComponent<Collider>();
        if (collider == null || !collider.isTrigger)
        {
            failures.Add(path + " is missing a trigger collider.");
        }
        if (prefab.GetComponentsInChildren<MeshRenderer>(true).Length < minimumMeshRenderers)
        {
            failures.Add(path + " does not contain enough obstacle visual mesh parts.");
        }
        if (prefab.GetComponentInChildren<TextMesh>(true) == null)
        {
            failures.Add(path + " is missing an obstacle health label.");
        }
    }

    private static void ValidatePooledPrefab(List<string> failures, string path, params System.Type[] requiredComponents)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            failures.Add("Missing pooled prefab: " + path);
            return;
        }

        if (prefab.GetComponent<PooledObject>() == null)
        {
            failures.Add(path + " is missing PooledObject.");
        }

        foreach (System.Type requiredComponent in requiredComponents)
        {
            if (prefab.GetComponent(requiredComponent) == null)
            {
                failures.Add(path + " is missing required pooled component " + requiredComponent.Name + ".");
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

        ValidateInputModules(path, failures);
        validate(scene, failures);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static void ValidateInputModules(string path, List<string> failures)
    {
        UnityEngine.EventSystems.StandaloneInputModule[] legacyModules = Object.FindObjectsByType<UnityEngine.EventSystems.StandaloneInputModule>(FindObjectsInactive.Include);
        if (legacyModules.Length > 0)
        {
            failures.Add($"{path} uses StandaloneInputModule, which throws at runtime when Player Settings use Input System package input.");
        }

        UnityEngine.EventSystems.EventSystem[] eventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include);
        foreach (UnityEngine.EventSystems.EventSystem eventSystem in eventSystems)
        {
            if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                failures.Add($"{path}/{eventSystem.name} is missing InputSystemUIInputModule.");
            }
        }
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

        int upgradeButtonCount = Object.FindObjectsByType<UpgradeButtonView>(FindObjectsInactive.Include).Length;
        int requiredUpgradeCount = System.Enum.GetValues(typeof(UpgradeType)).Length;
        if (upgradeButtonCount < requiredUpgradeCount)
        {
            failures.Add($"MainMenu exposes {upgradeButtonCount} upgrade buttons, but {requiredUpgradeCount} upgrade types exist.");
        }

        ValidateMainMenuShowcase(failures);
        ValidateMainMenuUpgradeLayout(failures);
    }

    private static void ValidateMainMenuShowcase(List<string> failures)
    {
        GameObject showcaseRoot = GameObject.Find("MenuShowcaseRoot");
        if (showcaseRoot == null)
        {
            failures.Add("MainMenu scene is missing MenuShowcaseRoot.");
            return;
        }

        if (showcaseRoot.GetComponent<MenuShowcaseAnimator>() == null)
        {
            failures.Add("MenuShowcaseRoot is missing MenuShowcaseAnimator.");
        }

        int soldierCount = 0;
        foreach (Transform child in showcaseRoot.transform)
        {
            if (child.name.StartsWith("HeroSoldier_"))
            {
                soldierCount++;
            }
        }

        if (soldierCount < 5)
        {
            failures.Add("MainMenu character showcase has fewer than five hero soldiers.");
        }

        int meshRendererCount = showcaseRoot.GetComponentsInChildren<MeshRenderer>(true).Length;
        if (meshRendererCount < 24)
        {
            failures.Add("MainMenu character showcase does not contain enough rendered visual parts.");
        }
    }

    private static void ValidateMainMenuUpgradeLayout(List<string> failures)
    {
        foreach (UpgradeButtonView view in Object.FindObjectsByType<UpgradeButtonView>(FindObjectsInactive.Include))
        {
            Transform root = view.transform;
            if (!TryGetChildRect(root, "UpgradeIcon", out RectTransform iconRect))
            {
                failures.Add(root.name + " is missing its upgrade icon.");
                continue;
            }

            if (!TryGetChildRect(root, "Title", out RectTransform titleRect) ||
                !TryGetChildRect(root, "Level", out RectTransform levelRect) ||
                !TryGetChildRect(root, "Cost", out RectTransform costRect))
            {
                failures.Add(root.name + " is missing required upgrade text labels.");
                continue;
            }

            if (RectTransformsOverlap(iconRect, titleRect) || RectTransformsOverlap(iconRect, levelRect))
            {
                failures.Add(root.name + " upgrade icon overlaps its text labels.");
            }

            if (RectTransformsOverlap(levelRect, costRect))
            {
                failures.Add(root.name + " upgrade level and cost labels overlap.");
            }
        }
    }

    private static bool TryGetChildRect(Transform parent, string childName, out RectTransform rect)
    {
        Transform child = parent.Find(childName);
        if (child != null && child.TryGetComponent(out rect))
        {
            return true;
        }

        rect = null;
        return false;
    }

    private static bool RectTransformsOverlap(RectTransform first, RectTransform second)
    {
        Rect firstRect = GetWorldRect(first);
        Rect secondRect = GetWorldRect(second);
        return firstRect.Overlaps(secondRect, true);
    }

    private static Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    }

    private static void ValidateLevelDataAssets(List<string> failures)
    {
        LevelData[] levels = AssetDatabase.FindAssets("t:LevelData", new[] { LevelDataPath })
            .Select(guid => AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => asset != null)
            .OrderBy(asset => asset.levelIndex)
            .ToArray();

        if (levels.Length < 20)
        {
            failures.Add("Expected at least 20 authored level assets.");
        }

        GlobalTuning tuning = AssetDatabase.LoadAssetAtPath<GlobalTuning>(TuningPath + "/SO_GlobalTuning.asset");
        int defaultStartingSoldiers = tuning != null ? Mathf.Max(1, tuning.defaultStartingSoldiers) : 10;
        float trackHalfWidth = tuning != null ? Mathf.Max(1f, tuning.trackHalfWidth) : 3.2f;
        HashSet<int> seenIndices = new HashSet<int>();
        HashSet<ObstacleDefinition> referencedObstacleDefinitions = new HashSet<ObstacleDefinition>();
        int bossLevelCount = 0;

        foreach (LevelData level in levels)
        {
            string label = level.name + " (Level " + level.levelIndex + ")";
            if (!seenIndices.Add(level.levelIndex))
            {
                failures.Add(label + " duplicates another authored level index.");
            }
            if (level.levelIndex <= 0)
            {
                failures.Add(label + " has an invalid level index.");
            }
            if (level.trackLength < 80f)
            {
                failures.Add(label + " has an unusually short track length.");
            }
            if (level.baseCoinReward <= 0)
            {
                failures.Add(label + " has no base coin reward.");
            }
            if (level.difficultyRating <= 0)
            {
                failures.Add(label + " has no difficulty rating.");
            }
            if (level.bonusCrateCount <= 0 || level.bonusCrateHealth <= 0 || level.bonusCrateReward <= 0 || level.bonusSectionLength <= 0f)
            {
                failures.Add(label + " has invalid bonus-run reward data.");
            }
            if (level.gates == null || level.gates.Count < 2)
            {
                failures.Add(label + " has fewer than two gates.");
            }
            else if (EstimateBestGatePath(level, defaultStartingSoldiers) <= 0)
            {
                failures.Add(label + " has no positive gate path from the starting troop count.");
            }

            ValidateGateData(failures, level, label, trackHalfWidth);
            ValidateEnemyData(failures, level, label, trackHalfWidth);
            ValidateObstacleData(failures, level, label, trackHalfWidth, referencedObstacleDefinitions);

            if (level.hasBoss)
            {
                bossLevelCount++;
                if (level.bossDefinition == null)
                {
                    failures.Add(label + " is marked as a boss level without a BossDefinition.");
                }
            }
        }

        if (bossLevelCount < 4)
        {
            failures.Add("Expected at least four authored boss levels.");
        }
        if (referencedObstacleDefinitions.Count < 7)
        {
            failures.Add("Authored levels do not reference every required obstacle variant.");
        }
    }

    private static void ValidateGateData(List<string> failures, LevelData level, string label, float trackHalfWidth)
    {
        if (level.gates == null)
        {
            return;
        }

        for (int i = 0; i < level.gates.Count; i++)
        {
            GateSpawnData gate = level.gates[i];
            if (gate == null)
            {
                failures.Add(label + " has a null gate entry.");
                continue;
            }

            if (gate.z <= 0f || gate.z >= level.trackLength)
            {
                failures.Add(label + " has gate " + i + " outside the playable track.");
            }
            if (Mathf.Abs(gate.x) > trackHalfWidth)
            {
                failures.Add(label + " has gate " + i + " outside the track width.");
            }
            if (gate.value <= 0)
            {
                failures.Add(label + " has gate " + i + " with a non-positive value.");
            }
        }
    }

    private static void ValidateEnemyData(List<string> failures, LevelData level, string label, float trackHalfWidth)
    {
        if (level.enemyGroups == null)
        {
            return;
        }

        for (int i = 0; i < level.enemyGroups.Count; i++)
        {
            EnemyGroupSpawnData enemy = level.enemyGroups[i];
            if (enemy == null)
            {
                failures.Add(label + " has a null enemy entry.");
                continue;
            }

            if (enemy.z <= 0f || enemy.z >= level.trackLength)
            {
                failures.Add(label + " has enemy group " + i + " outside the playable track.");
            }
            if (Mathf.Abs(enemy.x) > trackHalfWidth)
            {
                failures.Add(label + " has enemy group " + i + " outside the track width.");
            }
            if (enemy.count <= 0 || enemy.healthPerUnit <= 0)
            {
                failures.Add(label + " has enemy group " + i + " with invalid combat values.");
            }
            if (level.hasBoss && enemy.z >= GetBossApproachStart(level) - 4f)
            {
                failures.Add(label + " has enemy group " + i + " too close to the boss approach zone.");
            }
        }
    }

    private static void ValidateObstacleData(List<string> failures, LevelData level, string label, float trackHalfWidth, HashSet<ObstacleDefinition> referencedObstacleDefinitions)
    {
        if (level.obstacles == null)
        {
            return;
        }

        for (int i = 0; i < level.obstacles.Count; i++)
        {
            ObstacleSpawnData obstacle = level.obstacles[i];
            if (obstacle == null)
            {
                failures.Add(label + " has a null obstacle entry.");
                continue;
            }

            if (obstacle.z <= 0f || obstacle.z >= level.trackLength)
            {
                failures.Add(label + " has obstacle " + i + " outside the playable track.");
            }
            if (Mathf.Abs(obstacle.x) > trackHalfWidth)
            {
                failures.Add(label + " has obstacle " + i + " outside the track width.");
            }
            if (obstacle.health <= 0 || obstacle.collisionPenalty < 0)
            {
                failures.Add(label + " has obstacle " + i + " with invalid combat values.");
            }
            if (level.hasBoss && obstacle.z >= GetBossApproachStart(level) - 4f)
            {
                failures.Add(label + " has obstacle " + i + " too close to the boss approach zone.");
            }
            if (obstacle.definition == null)
            {
                failures.Add(label + " has obstacle " + i + " without an ObstacleDefinition.");
            }
            else
            {
                referencedObstacleDefinitions.Add(obstacle.definition);
                if (obstacle.definition.prefab == null)
                {
                    failures.Add(label + " has obstacle " + i + " using an ObstacleDefinition without a prefab.");
                }
                if (obstacle.definition.width <= 0f)
                {
                    failures.Add(label + " has obstacle " + i + " using an ObstacleDefinition with invalid width.");
                }
            }
        }
    }

    private static float GetBossApproachStart(LevelData level)
    {
        if (level == null || !level.hasBoss)
        {
            return float.PositiveInfinity;
        }

        float activationDistance = level.bossDefinition != null ? Mathf.Max(1f, level.bossDefinition.activationDistance) : 18f;
        return level.trackLength - 24f - activationDistance;
    }

    private static int EstimateBestGatePath(LevelData level, int startingSoldiers)
    {
        int count = Mathf.Max(1, level.startingSoldiersOverride > 0 ? level.startingSoldiersOverride : startingSoldiers);
        List<GateSpawnData> gates = level.gates
            .Where(gate => gate != null)
            .OrderBy(gate => gate.z)
            .ToList();

        for (int i = 0; i < gates.Count;)
        {
            float z = gates[i].z;
            int best = count;
            while (i < gates.Count && Mathf.Abs(gates[i].z - z) <= 0.1f)
            {
                best = Mathf.Max(best, ApplyGateEstimate(count, gates[i]));
                i++;
            }

            count = best;
        }

        return count;
    }

    private static int ApplyGateEstimate(int count, GateSpawnData gate)
    {
        switch (gate.operation)
        {
            case GateOperation.Add:
                return count + gate.value;
            case GateOperation.Subtract:
                return Mathf.Max(0, count - gate.value);
            case GateOperation.Multiply:
                return count * Mathf.Max(1, gate.value);
            case GateOperation.Divide:
                return Mathf.FloorToInt(count / Mathf.Max(1f, gate.value));
            default:
                return count;
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
        CameraFollowRig cameraRig = Object.FindAnyObjectByType<CameraFollowRig>();
        Camera mainCamera = Camera.main;

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
        if (cameraRig == null)
        {
            failures.Add("Game scene is missing CameraFollowRig.");
        }
        if (mainCamera == null)
        {
            failures.Add("Game scene is missing a tagged main camera.");
        }
        else if (cameraRig != null)
        {
            if (mainCamera.transform.parent != cameraRig.transform)
            {
                failures.Add("Main camera is not parented to the camera follow rig.");
            }
            if (mainCamera.transform.localPosition.sqrMagnitude > 0.001f || Quaternion.Angle(mainCamera.transform.localRotation, Quaternion.identity) > 0.1f)
            {
                failures.Add("Main camera child transform must be local-zero under the follow rig to avoid double-offset framing.");
            }
        }
        string[] requiredVfx =
        {
            PrefabPath + "/VFX/PF_VFX_MuzzleFlash.prefab",
            PrefabPath + "/VFX/PF_VFX_HitSpark.prefab",
            PrefabPath + "/VFX/PF_VFX_GatePositive.prefab",
            PrefabPath + "/VFX/PF_VFX_GateNegative.prefab",
            PrefabPath + "/VFX/PF_VFX_CrowdGain.prefab",
            PrefabPath + "/VFX/PF_VFX_CrowdLoss.prefab",
            PrefabPath + "/VFX/PF_VFX_CoinBurst.prefab",
            PrefabPath + "/VFX/PF_VFX_ObstacleDebris.prefab",
            PrefabPath + "/VFX/PF_VFX_VictoryBurst.prefab",
            PrefabPath + "/VFX/PF_VFX_BossExplosion.prefab"
        };
        foreach (string vfxPath in requiredVfx)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(vfxPath) == null)
            {
                failures.Add("Missing VFX prefab: " + vfxPath);
            }
        }
        if (pool == null)
        {
            failures.Add("Game scene is missing PoolManager.");
        }

        levelManager.BuildCurrentLevel();
        if (pool != null)
        {
            PooledObject[] activePooledObjects = Object.FindObjectsByType<PooledObject>(FindObjectsInactive.Exclude);
            foreach (PooledObject pooledObject in activePooledObjects)
            {
                if (pooledObject.Owner == null)
                {
                    failures.Add(pooledObject.name + " is active without a PoolManager owner.");
                }
            }
        }

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
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/Bosses/PF_Boss_Helicopter.prefab") == null)
        {
            failures.Add("Missing helicopter boss prefab.");
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/Bosses/PF_Boss_Mech.prefab") == null)
        {
            failures.Add("Missing mech boss prefab.");
        }
        if (AssetDatabase.LoadAssetAtPath<BossDefinition>(BossDataPath + "/SO_Boss_Tank.asset") == null)
        {
            failures.Add("Missing tank boss definition.");
        }
        if (AssetDatabase.LoadAssetAtPath<BossDefinition>(BossDataPath + "/SO_Boss_Helicopter.asset") == null)
        {
            failures.Add("Missing helicopter boss definition.");
        }
        if (AssetDatabase.LoadAssetAtPath<BossDefinition>(BossDataPath + "/SO_Boss_Mech.asset") == null)
        {
            failures.Add("Missing mech boss definition.");
        }
        LevelData[] levelAssets = AssetDatabase.FindAssets("t:LevelData", new[] { LevelDataPath })
            .Select(guid => AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => asset != null)
            .ToArray();
        int endlessPreviewIndex = levelAssets.Length > 0 ? levelAssets.Max(level => level.levelIndex) + 1 : 21;
        LevelData endlessPreview = levelManager.PreviewLevelData(endlessPreviewIndex);
        if (endlessPreview == null || endlessPreview.levelIndex != endlessPreviewIndex)
        {
            failures.Add("LevelManager did not create a deterministic endless level preview.");
        }
        else if (endlessPreview.gates.Count < 6 || endlessPreview.enemyGroups.Count < 2 || endlessPreview.obstacles.Count < 2 || endlessPreview.baseCoinReward <= 0)
        {
            failures.Add("Endless level preview is missing required scaled encounters or rewards.");
        }
        if (levelAssets.Any(level => level.hasBoss && level.bossDefinition == null))
        {
            failures.Add("One or more boss levels are missing a BossDefinition reference.");
        }
        if (levelAssets.Where(level => level.hasBoss && level.bossDefinition != null).Select(level => level.bossDefinition).Distinct().Count() < 3)
        {
            failures.Add("Authored boss levels do not rotate across all three boss definitions.");
        }
        if (levelAssets.Any(level => level.hasBoss && level.bossDefinition != null && level.bossDefinition.bossPrefab == null))
        {
            failures.Add("One or more boss definitions is missing its visual prefab reference.");
        }
        if (Object.FindAnyObjectByType<FinishLineTrigger>() == null)
        {
            failures.Add("Game scene validation spawned no finish trigger.");
        }
        if (Object.FindObjectsByType<BonusCrateController>(FindObjectsInactive.Exclude).Length == 0)
        {
            failures.Add("Game scene validation spawned no bonus crates.");
        }
        if (Object.FindAnyObjectByType<BonusEndTrigger>() == null)
        {
            failures.Add("Game scene validation spawned no bonus end trigger.");
        }
    }

    private static void ConfigurePlayerSettings()
    {
        ConfigurePlayerSettings(iOSSdkVersion.DeviceSDK);
    }

    private static void ConfigurePlayerSettings(iOSSdkVersion sdkVersion)
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
        PlayerSettings.iOS.sdkVersion = sdkVersion;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.armyrush.game");
        ConfigureAppIcons();
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
        UnityEngine.EventSystems.EventSystem eventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        GameObject eventSystemObject;
        if (eventSystem == null)
        {
            eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }
        else
        {
            eventSystemObject = eventSystem.gameObject;
        }

        UnityEngine.EventSystems.StandaloneInputModule[] legacyModules = eventSystemObject.GetComponents<UnityEngine.EventSystems.StandaloneInputModule>();
        foreach (UnityEngine.EventSystems.StandaloneInputModule legacyModule in legacyModules)
        {
            Object.DestroyImmediate(legacyModule);
        }

        UnityEngine.InputSystem.UI.InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystemObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
        }
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

    private static Image CreateFullscreenImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = obj.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
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

    private static GameObject CreateResultPanel(string name, Transform parent, string header, out Text coinsText, out Button actionButton, out Button upgradeButton, out Button placeholderButton, out Text statusText)
    {
        GameObject panel = CreatePanel(name, parent, new Vector2(0.5f, 0.48f), new Vector2(720f, 520f), new Color(0.04f, 0.11f, 0.22f, 0.94f));
        panel.AddComponent<ResultPanelAnimator>();
        CreateUIText("Header", panel.transform, header, 68, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.78f), new Vector2(620f, 100f));
        coinsText = CreateUIText("Coins", panel.transform, "+0 COINS", 44, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.12f), new Vector2(0.5f, 0.55f), new Vector2(620f, 80f));
        placeholderButton = CreateButton(header == "VICTORY" ? "RewardedButton" : "ReviveButton", panel.transform, header == "VICTORY" ? "2X REWARD" : "REVIVE", new Vector2(0.5f, 0.36f), new Vector2(620f, 70f), new Color(0.05f, 0.13f, 0.24f));
        statusText = CreateUIText("StatusText", panel.transform, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.12f), new Vector2(0.5f, 0.27f), new Vector2(620f, 44f));
        actionButton = CreateButton("ActionButton", panel.transform, header == "VICTORY" ? "NEXT" : "RETRY", new Vector2(0.32f, 0.16f), new Vector2(286f, 78f), new Color(0.05f, 0.75f, 0.35f));
        upgradeButton = CreateButton("UpgradeButton", panel.transform, "UPGRADES", new Vector2(0.68f, 0.16f), new Vector2(286f, 78f), new Color(0.08f, 0.28f, 0.95f));
        return panel;
    }

    private static void CreateSettingsPanel(Transform parent, SettingsPanelUI settings)
    {
        GameObject panel = CreatePanel("SettingsPanel", parent, new Vector2(0.5f, 0.52f), new Vector2(720f, 680f), new Color(0.04f, 0.11f, 0.22f, 0.96f));
        CreateUIText("Header", panel.transform, "SETTINGS", 58, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.86f), new Vector2(620f, 90f));
        CreateUIText("MusicLabel", panel.transform, "MUSIC", 34, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.24f, 0.72f), new Vector2(220f, 58f));
        Slider musicSlider = CreateInteractiveSlider("MusicSlider", panel.transform, new Vector2(0.62f, 0.72f), new Vector2(390f, 34f));
        CreateUIText("SfxLabel", panel.transform, "SFX", 34, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.24f, 0.58f), new Vector2(220f, 58f));
        Slider sfxSlider = CreateInteractiveSlider("SfxSlider", panel.transform, new Vector2(0.62f, 0.58f), new Vector2(390f, 34f));
        Toggle hapticsToggle = CreateToggle("HapticsToggle", panel.transform, "HAPTICS", new Vector2(0.5f, 0.44f), new Vector2(540f, 78f));
        Button privacy = CreateButton("PrivacyButton", panel.transform, "PRIVACY", new Vector2(0.33f, 0.29f), new Vector2(250f, 56f), new Color(0.05f, 0.13f, 0.24f));
        Button terms = CreateButton("TermsButton", panel.transform, "TERMS", new Vector2(0.67f, 0.29f), new Vector2(250f, 56f), new Color(0.05f, 0.13f, 0.24f));
        Button restore = CreateButton("RestoreButton", panel.transform, "RESTORE PURCHASES", new Vector2(0.5f, 0.2f), new Vector2(540f, 62f), new Color(0.08f, 0.28f, 0.95f));
        Text restoreStatus = CreateUIText("RestoreStatusText", panel.transform, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.125f), new Vector2(560f, 42f));
        Button close = CreateButton("CloseButton", panel.transform, "CLOSE", new Vector2(0.5f, 0.065f), new Vector2(360f, 76f), new Color(0.08f, 0.28f, 0.95f));
        UnityEventTools.AddPersistentListener(close.onClick, settings.Close);
        panel.SetActive(false);

        SetObject(settings, "_panel", panel);
        SetObject(settings, "_musicSlider", musicSlider);
        SetObject(settings, "_sfxSlider", sfxSlider);
        SetObject(settings, "_hapticsToggle", hapticsToggle);
        SetObject(settings, "_privacyButton", privacy);
        SetObject(settings, "_termsButton", terms);
        SetObject(settings, "_restoreButton", restore);
        SetObject(settings, "_restoreStatusText", restoreStatus);
        SetObject(settings, "_closeButton", close);
    }

    private static Slider CreateInteractiveSlider(string name, Transform parent, Vector2 anchorPosition, Vector2 size)
    {
        GameObject root = CreatePanel(name, parent, anchorPosition, size, new Color(0.02f, 0.06f, 0.11f, 0.92f));
        Slider slider = root.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = true;

        GameObject fill = CreatePanel("Fill", root.transform, new Vector2(0f, 0.5f), size, new Color(0.1f, 0.86f, 0.36f, 1f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        slider.fillRect = fillRect;

        GameObject handle = CreatePanel("Handle", root.transform, new Vector2(1f, 0.5f), new Vector2(52f, 52f), Color.white);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(1f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        return slider;
    }

    private static Toggle CreateToggle(string name, Transform parent, string label, Vector2 anchorPosition, Vector2 size)
    {
        GameObject root = CreatePanel(name, parent, anchorPosition, size, new Color(0.02f, 0.06f, 0.11f, 0.92f));
        Toggle toggle = root.AddComponent<Toggle>();
        Image background = root.GetComponent<Image>();
        toggle.targetGraphic = background;

        GameObject check = CreatePanel("Checkmark", root.transform, new Vector2(0.16f, 0.5f), new Vector2(46f, 46f), new Color(0.1f, 0.86f, 0.36f, 1f));
        toggle.graphic = check.GetComponent<Image>();
        CreateUIText("Label", root.transform, label, 32, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(0.62f, 0.5f), new Vector2(360f, 60f));
        toggle.isOn = true;
        return toggle;
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

    private static Transform UpsertEmptyTransform(Transform parent, string name, Vector3 localPosition, Vector3 localEuler, Vector3 localScale)
    {
        Transform existing = parent.Find(name);
        GameObject obj = existing != null ? existing.gameObject : new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.Euler(localEuler);
        obj.transform.localScale = localScale;
        return obj.transform;
    }

    private static Transform UpsertMeshPart(Transform parent, string name, Mesh mesh, Material material, Vector3 localPosition, Vector3 localScale)
    {
        Transform transform = UpsertEmptyTransform(parent, name, localPosition, Vector3.zero, localScale);
        MeshFilter filter = transform.GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = transform.gameObject.AddComponent<MeshFilter>();
        }

        filter.sharedMesh = mesh;

        MeshRenderer renderer = transform.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = transform.gameObject.AddComponent<MeshRenderer>();
        }

        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return transform;
    }

    private static T LoadRequiredAsset<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new System.Exception($"Required asset missing: {path}");
        }

        return asset;
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
        material.enableInstancing = true;
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

    private static ObstacleDefinition CreateObstacleDefinition(
        string assetName,
        string displayName,
        ObstacleKind kind,
        GameObject prefab,
        int baseHealth,
        int healthPerLevel,
        int collisionPenalty,
        int baseCoinReward,
        int coinRewardPerLevel,
        float width,
        bool explosive)
    {
        string path = $"{ObstacleDataPath}/{assetName}.asset";
        ObstacleDefinition definition = AssetDatabase.LoadAssetAtPath<ObstacleDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<ObstacleDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }

        definition.displayName = displayName;
        definition.kind = kind;
        definition.prefab = prefab;
        definition.baseHealth = baseHealth;
        definition.healthPerLevel = healthPerLevel;
        definition.collisionPenalty = collisionPenalty;
        definition.baseCoinReward = baseCoinReward;
        definition.coinRewardPerLevel = coinRewardPerLevel;
        definition.width = width;
        definition.explosive = explosive;
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static BossDefinition CreateBossDefinition(
        string assetName,
        string displayName,
        GameObject bossPrefab,
        BossAttackPattern attackPattern,
        int baseHealth,
        int healthPerLevel,
        int collisionPenalty,
        int coinReward,
        int coinRewardPerLevel,
        float activationDistance,
        float attackInterval,
        float warningDuration,
        float attackRadius,
        float lateralRange,
        int attackDamage,
        int attackDamagePerLevel,
        string warningText,
        Color warningColor)
    {
        string path = $"{BossDataPath}/{assetName}.asset";
        BossDefinition definition = AssetDatabase.LoadAssetAtPath<BossDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<BossDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }

        definition.displayName = displayName;
        definition.bossPrefab = bossPrefab;
        definition.attackPattern = attackPattern;
        definition.baseHealth = baseHealth;
        definition.healthPerLevel = healthPerLevel;
        definition.collisionPenalty = collisionPenalty;
        definition.coinReward = coinReward;
        definition.coinRewardPerLevel = coinRewardPerLevel;
        definition.activationDistance = activationDistance;
        definition.attackInterval = attackInterval;
        definition.warningDuration = warningDuration;
        definition.attackRadius = attackRadius;
        definition.lateralRange = lateralRange;
        definition.attackDamage = attackDamage;
        definition.attackDamagePerLevel = attackDamagePerLevel;
        definition.warningText = warningText;
        definition.warningColor = warningColor;
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

    private static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
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
        public Material vfxParticle;
    }

    private sealed class MeshSet
    {
        public Mesh box;
        public Mesh panel;
        public Mesh cylinder;
        public Mesh road;
        public Mesh wedge;
    }

    private sealed class UiSpriteSet
    {
        public Sprite buttonFrame;
        public Sprite panelFrame;
        public Sprite coinIcon;
        public Sprite settingsIcon;
        public Sprite playIcon;
        public Sprite upgradeIcon;
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
        public GameObject obstacleCrateStack;
        public GameObject obstacleBarrelCluster;
        public GameObject obstacleConcreteBlock;
        public GameObject obstacleMilitaryTruck;
        public GameObject obstacleTurret;
        public GameObject obstacleFuelTank;
        public GameObject[] obstacleVariants;
        public GameObject bossTank;
        public GameObject bossHelicopter;
        public GameObject bossMech;
        public GameObject finishLine;
        public GameObject bonusCrate;
        public GameObject bonusEnd;
        public GameObject floatingText;
        public GameObject muzzleFlash;
        public GameObject hitSpark;
        public GameObject gatePositiveBurst;
        public GameObject gateNegativeBurst;
        public GameObject crowdGainBurst;
        public GameObject crowdLossBurst;
        public GameObject coinBurst;
        public GameObject obstacleDebris;
        public GameObject obstacleExplosion;
        public GameObject victoryBurst;
        public GameObject bossExplosion;
        public GameObject smokePuff;
        public GameObject heavySmoke;
    }
}
