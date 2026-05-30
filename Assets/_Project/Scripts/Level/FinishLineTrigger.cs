using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class FinishLineTrigger : MonoBehaviour
    {
        private RunManager _runManager;
        private bool _triggered;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        public void Configure(RunManager runManager)
        {
            _runManager = runManager;
            _triggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || other.GetComponentInParent<CrowdManager>() == null)
            {
                return;
            }

            _triggered = true;
            _runManager?.BeginFinishSequence();
        }
    }
}
