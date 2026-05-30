using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyRush
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static bool Has<T>() where T : class
        {
            return Services.ContainsKey(typeof(T));
        }

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"Cannot register null service {typeof(T).Name}.");
                return;
            }

            Services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (Services.TryGetValue(typeof(T), out object value))
            {
                return value as T;
            }

            Debug.LogError($"Service {typeof(T).Name} is not registered.");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out object value))
            {
                service = value as T;
                return service != null;
            }

            service = null;
            return false;
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
