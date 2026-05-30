using UnityEngine;

namespace ArmyRush
{
    public enum CameraShakeCue
    {
        ObstacleBreak,
        BossHit,
        BossDefeat,
        Victory
    }

    public sealed class CameraFollowRig : MonoBehaviour
    {
        [SerializeField] private GlobalTuning _tuning;
        [SerializeField] private Transform _target;
        [SerializeField] private Camera _camera;

        private Vector3 _velocity;
        private float _shakeTime;
        private float _shakeAmplitude;

        private static CameraFollowRig _active;

        private void Awake()
        {
            _active = this;
            if (_camera == null)
            {
                _camera = GetComponentInChildren<Camera>();
            }
        }

        private void OnDestroy()
        {
            if (_active == this)
            {
                _active = null;
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

        public static void Shake(CameraShakeCue cue)
        {
            if (_active == null)
            {
                _active = FindAnyObjectByType<CameraFollowRig>();
            }

            if (_active == null)
            {
                return;
            }

            switch (cue)
            {
                case CameraShakeCue.ObstacleBreak:
                    _active.Shake(0.08f, 0.12f);
                    break;
                case CameraShakeCue.BossHit:
                    _active.Shake(0.13f, 0.18f);
                    break;
                case CameraShakeCue.BossDefeat:
                    _active.Shake(0.22f, 0.26f);
                    break;
                case CameraShakeCue.Victory:
                    _active.Shake(0.1f, 0.2f);
                    break;
            }
        }
    }
}
