using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private RunnerInputController _input;
        [SerializeField] private CrowdManager _crowdManager;
        [SerializeField] private RunManager _runManager;

        private float _targetX;
        private float _xVelocity;

        public CrowdManager Crowd => _crowdManager;
        public float ProgressZ => transform.position.z;

        private void Reset()
        {
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void Awake()
        {
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            _targetX = transform.position.x;
        }

        private void Update()
        {
            if (_input == null || _tuning == null)
            {
                return;
            }

            if (_runManager != null && _runManager.State == RunState.PreRun && _input.ConsumeStartPressed())
            {
                _runManager.BeginRun();
            }

            if (_runManager == null || (_runManager.State != RunState.Running && _runManager.State != RunState.CombatPaused))
            {
                return;
            }

            float deltaX = _input.ConsumeDeltaX();
            _targetX = Mathf.Clamp(_targetX + deltaX * _tuning.lateralSensitivity, -_tuning.trackHalfWidth, _tuning.trackHalfWidth);

            Vector3 position = transform.position;
            if (_runManager.State == RunState.Running)
            {
                position.z += _tuning.forwardSpeed * Time.deltaTime;
            }
            position.x = Mathf.SmoothDamp(position.x, _targetX, ref _xVelocity, _tuning.lateralSmoothTime);
            transform.position = position;
        }

        public void Configure(GlobalTuning tuning, RunnerInputController input, CrowdManager crowdManager, RunManager runManager)
        {
            _tuning = tuning;
            _input = input;
            _crowdManager = crowdManager;
            _runManager = runManager;
        }
    }
}
