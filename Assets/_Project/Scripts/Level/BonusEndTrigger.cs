using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class BonusEndTrigger : MonoBehaviour
    {
        private RunManager _runManager;
        private bool _triggered;

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
            if (_triggered || other.GetComponentInParent<CrowdManager>() == null)
            {
                return;
            }

            _triggered = true;
            _runManager?.WinRun();
        }
    }
}
