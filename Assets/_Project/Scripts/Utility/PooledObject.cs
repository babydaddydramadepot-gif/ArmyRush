using UnityEngine;

namespace ArmyRush
{
    public sealed class PooledObject : MonoBehaviour
    {
        public PoolManager Owner { get; private set; }
        public GameObject SourcePrefab { get; private set; }

        public void Initialize(PoolManager owner, GameObject sourcePrefab)
        {
            Owner = owner;
            SourcePrefab = sourcePrefab;
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
