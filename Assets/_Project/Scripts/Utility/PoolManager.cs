using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public sealed class PoolManager : MonoBehaviour
    {
        private readonly Dictionary<GameObject, Queue<PooledObject>> _pools = new Dictionary<GameObject, Queue<PooledObject>>();

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                PooledObject pooled = Create(prefab);
                pooled.gameObject.SetActive(false);
                GetQueue(prefab).Enqueue(pooled);
            }
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            PooledObject pooled = Get(prefab, position, rotation, parent);
            return pooled.GetComponent<T>();
        }

        public PooledObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Queue<PooledObject> queue = GetQueue(prefab);
            PooledObject pooled = queue.Count > 0 ? queue.Dequeue() : Create(prefab);
            Transform pooledTransform = pooled.transform;
            pooledTransform.SetParent(parent, false);
            pooledTransform.SetPositionAndRotation(position, rotation);
            pooled.gameObject.SetActive(true);
            return pooled;
        }

        public void Release(PooledObject pooled)
        {
            if (pooled == null)
            {
                return;
            }

            pooled.gameObject.SetActive(false);
            pooled.transform.SetParent(transform, false);

            if (pooled.SourcePrefab == null)
            {
                return;
            }

            GetQueue(pooled.SourcePrefab).Enqueue(pooled);
        }

        private PooledObject Create(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.name = prefab.name;
            PooledObject pooled = instance.GetComponent<PooledObject>();
            if (pooled == null)
            {
                pooled = instance.AddComponent<PooledObject>();
            }

            pooled.Initialize(this, prefab);
            return pooled;
        }

        private Queue<PooledObject> GetQueue(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Queue<PooledObject> queue))
            {
                queue = new Queue<PooledObject>();
                _pools[prefab] = queue;
            }

            return queue;
        }
    }
}
