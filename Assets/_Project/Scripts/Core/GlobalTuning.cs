using UnityEngine;

namespace ArmyRush
{
    [CreateAssetMenu(menuName = "ArmyRush/Tuning/Global Tuning", fileName = "SO_GlobalTuning")]
    public sealed class GlobalTuning : ScriptableObject
    {
        [Header("Player")]
        public float forwardSpeed = 7.5f;
        public float lateralSensitivity = 0.018f;
        public float lateralSmoothTime = 0.08f;
        public float trackHalfWidth = 3.2f;

        [Header("Crowd")]
        public int defaultStartingSoldiers = 10;
        public int maxVisualSoldiers = 180;
        public int hardSoldierCap = 300;
        public float soldierSpacingX = 0.48f;
        public float soldierSpacingZ = 0.46f;
        public int maxFormationColumns = 13;

        [Header("Combat")]
        public float targetRange = 24f;
        public float targetLateralRange = 3.8f;
        public float projectileSpeed = 36f;
        public float projectileLifetime = 1.6f;
        public float baseFireInterval = 0.3f;
        public float minFireInterval = 0.08f;
        public int baseDamage = 10;
        public int projectileVisualBurst = 6;
        public float openingVolleyDamageMultiplier = 1.35f;
        public int openingVolleyExtraProjectiles = 2;
        public float openingVolleyCooldown = 0.6f;

        [Header("Rewards")]
        public int soldierCoinValue = 2;
        public int enemyCoinValue = 2;
        public int obstacleCoinValue = 12;
        public int bossCoinValue = 250;
        public int earlyDefeatRewardLevelLimit = 5;
        [Range(0f, 1f)] public float earlyDefeatRewardFraction = 0.4f;
        public int earlyDefeatMinimumCoins = 100;
        public int earlyRallyAssistLevelLimit = 5;
        public int earlyRallyAssistMinimumSoldiers = 6;
        public int earlyRallyAssistTargetSoldiers = 12;
        public int earlyRallyAssistMaxUsesPerRun = 2;
        public float earlyRallyAssistCooldown = 4f;

        [Header("Camera")]
        public Vector3 cameraOffset = new Vector3(0f, 10f, -10.5f);
        public Vector3 cameraEuler = new Vector3(42f, 0f, 0f);
        public float cameraSmooth = 9f;
        public float cameraLookAhead = 5f;
        public float cameraFov = 52f;

        [Header("Camera Shake")]
        public float cameraShakeGlobalScale = 0.7f;
        public float cameraShakeMaxAmplitude = 0.16f;
        public float cameraShakeFrequency = 26f;
        public float obstacleShakeAmplitude = 0.055f;
        public float obstacleShakeDuration = 0.1f;
        public float bossHitShakeAmplitude = 0.085f;
        public float bossHitShakeDuration = 0.16f;
        public float bossDefeatShakeAmplitude = 0.145f;
        public float bossDefeatShakeDuration = 0.24f;
        public float victoryShakeAmplitude = 0.075f;
        public float victoryShakeDuration = 0.18f;
    }
}
