using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public sealed class PooledObject : MonoBehaviour
    {
        private Dictionary<Type, Component> _componentCache;

        public PoolManager Owner { get; private set; }
        public GameObject SourcePrefab { get; private set; }

        public void Initialize(PoolManager owner, GameObject sourcePrefab)
        {
            Owner = owner;
            SourcePrefab = sourcePrefab;
        }

        public T GetCachedComponent<T>() where T : Component
        {
            Type type = typeof(T);
            if (_componentCache != null && _componentCache.TryGetValue(type, out Component cached))
            {
                return cached as T;
            }

            T component = GetComponent<T>();
            if (component == null)
            {
                return null;
            }

            if (_componentCache == null)
            {
                _componentCache = new Dictionary<Type, Component>(2);
            }
            _componentCache[type] = component;
            return component;
        }

        public void Release()
        {
            if (Owner != null)
            {
                Owner.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
