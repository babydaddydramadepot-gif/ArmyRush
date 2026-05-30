using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public static class TargetRegistry
    {
        private static readonly List<Damageable> Targets = new List<Damageable>();

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

        public static Damageable FindBestTarget(Vector3 origin, float range, float lateralRange)
        {
            Damageable best = null;
            float bestScore = float.MaxValue;

            for (int i = Targets.Count - 1; i >= 0; i--)
            {
                Damageable target = Targets[i];
                if (target == null || !target.IsAlive)
                {
                    Targets.RemoveAt(i);
                    continue;
                }

                Vector3 offset = target.transform.position - origin;
                if (offset.z <= 0f || offset.z > range || Mathf.Abs(offset.x) > lateralRange)
                {
                    continue;
                }

                float priority = target.Kind == CombatTargetKind.Obstacle ? -8f : 0f;
                float score = offset.z + Mathf.Abs(offset.x) * 1.5f + priority;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = target;
                }
            }

            return best;
        }
    }
}
