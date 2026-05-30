using UnityEngine;

namespace ArmyRush
{
    public sealed class CameraFollowRig : MonoBehaviour
    {
        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private Transform _target;
        [SerializeField] private Camera _camera;

        private Vector3 _velocity;
        private float _shakeTime;
        private float _shakeAmplitude;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = GetComponentInChildren<Camera>();
            }
        }

        private void LateUpdate()
        {
            if (_target == null || _tuning == null)
            {
                return;
            }

            Vector3 desired = _target.position + _tuning.cameraOffset + Vector3.forward * _tuning.cameraLookAhead;
            Vector3 position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 1f / Mathf.Max(0.01f, _tuning.cameraSmooth));

            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.deltaTime;
                position += Random.insideUnitSphere * _shakeAmplitude;
                _shakeAmplitude = Mathf.MoveTowards(_shakeAmplitude, 0f, Time.deltaTime * 2f);
            }

            transform.position = position;
            transform.rotation = Quaternion.Euler(_tuning.cameraEuler);

            if (_camera != null)
            {
                _camera.fieldOfView = _tuning.cameraFov;
            }
        }

        public void Configure(GlobalTuning tuning, Transform target)
        {
            _tuning = tuning;
            _target = target;
        }

        public void Shake(float amplitude, float duration)
        {
            _shakeAmplitude = Mathf.Max(_shakeAmplitude, amplitude);
            _shakeTime = Mathf.Max(_shakeTime, duration);
        }
    }
}
