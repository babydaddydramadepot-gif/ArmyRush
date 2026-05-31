using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public static class TargetRegistry
    {
        private static readonly List<Damageable> Targets = new List<Damageable>();

        public static int RegisteredCount => Targets.Count;

        public static void Register(Damageable target)
        {
            if (target != null && !Targets.Contains(target))
            {
                Targets.Add(target);
            }
        }

        public static void Unregister(Damageable target)
        {
            Targets.Remove(target);
        }

        public static void Clear()
        {
            Targets.Clear();
        }

        public static Damageable FindBestTarget(Vector3 origin, float range, float lateralRange)
        {
            Damageable closestObstacle = null;
            Damageable closestEnemy = null;
            Damageable closestBoss = null;
            Damageable closestBonus = null;
            float closestObstacleScore = float.MaxValue;
            float closestEnemyScore = float.MaxValue;
            float closestBossScore = float.MaxValue;
            float closestBonusScore = float.MaxValue;

            for (int i = Targets.Count - 1; i >= 0; i--)
            {
                Damageable target = Targets[i];
                if (target == null || !target.IsAlive)
                {
                    Targets.RemoveAt(i);
                    continue;
                }
                if (!target.IsTargetable)
                {
                    continue;
                }

                Vector3 offset = target.transform.position - origin;
                if (offset.z <= 0f || offset.z > range || Mathf.Abs(offset.x) > lateralRange)
                {
                    continue;
                }

                float score = offset.z + Mathf.Abs(offset.x) * 1.5f;
                switch (target.Kind)
                {
                    case CombatTargetKind.Obstacle:
                        if (score < closestObstacleScore)
                        {
                            closestObstacleScore = score;
                            closestObstacle = target;
                        }
                        break;

                    case CombatTargetKind.Enemy:
                        if (score < closestEnemyScore)
                        {
                            closestEnemyScore = score;
                            closestEnemy = target;
                        }
                        break;

                    case CombatTargetKind.Boss:
                        if (score < closestBossScore)
                        {
                            closestBossScore = score;
                            closestBoss = target;
                        }
                        break;

                    case CombatTargetKind.Bonus:
                        if (score < closestBonusScore)
                        {
                            closestBonusScore = score;
                            closestBonus = target;
                        }
                        break;
                }
            }

            if (closestObstacle != null && (closestEnemy == null || closestObstacleScore <= closestEnemyScore))
            {
                return closestObstacle;
            }

            if (closestEnemy != null)
            {
                return closestEnemy;
            }

            if (closestObstacle != null)
            {
                return closestObstacle;
            }

            if (closestBoss != null)
            {
                return closestBoss;
            }

            return closestBonus;
        }
    }
}
