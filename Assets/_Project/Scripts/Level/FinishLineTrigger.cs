using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class FinishLineTrigger : MonoBehaviour
    {
        private RunManager _runManager;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Start()
        {
            _runManager = FindAnyObjectByType<RunManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<CrowdManager>() != null)
            {
                _runManager?.WinRun();
            }
        }
    }
}
