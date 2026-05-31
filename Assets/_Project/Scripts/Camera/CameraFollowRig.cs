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
        private Vector3 _lastTargetPosition;
        private float _shakeRemaining;
        private float _shakeDuration;
        private float _shakeAmplitude;
        private float _shakeSeed;
        private bool _hasTargetSample;

        private static CameraFollowRig _active;

        private void Awake()
        {
            _active = this;
            if (_camera == null)
            {
                _camera = GetComponentInChildren<Camera>();
            }
            Billboard.RegisterCamera(_camera);
            WorldTextGuard.ClampSceneText();
            NormalizeCameraChild();
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

            float dynamicLookAhead = GetDynamicLookAhead();
            Vector3 desired = _target.position + _tuning.cameraOffset + Vector3.forward * dynamicLookAhead;
            Vector3 position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 1f / Mathf.Max(0.01f, _tuning.cameraSmooth));
            Quaternion cameraRotation = Quaternion.Euler(_tuning.cameraEuler);

            position += GetShakeOffset(cameraRotation);

            transform.position = position;
            transform.rotation = cameraRotation;

            if (_camera != null)
            {
                _camera.fieldOfView = _tuning.cameraFov;
            }
        }

        public void Configure(GlobalTuning tuning, Transform target)
        {
            _tuning = tuning;
            _target = target;
            _hasTargetSample = false;
            NormalizeCameraChild();
        }

        public void Shake(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f)
            {
                return;
            }

            float scale = _tuning != null ? Mathf.Max(0f, _tuning.cameraShakeGlobalScale) : 1f;
            float maxAmplitude = _tuning != null ? Mathf.Max(0f, _tuning.cameraShakeMaxAmplitude) : 0.16f;
            float tunedAmplitude = Mathf.Min(amplitude * scale, maxAmplitude);
            if (tunedAmplitude <= 0f)
            {
                return;
            }

            _shakeAmplitude = Mathf.Max(_shakeAmplitude, tunedAmplitude);
            _shakeDuration = Mathf.Max(_shakeDuration, duration);
            _shakeRemaining = Mathf.Max(_shakeRemaining, duration);
            _shakeSeed += 0.37f;
        }

        private float GetDynamicLookAhead()
        {
            float lookAhead = _tuning.cameraLookAhead;
            if (_target == null || Time.deltaTime <= 0f)
            {
                return lookAhead;
            }

            if (!_hasTargetSample)
            {
                _lastTargetPosition = _target.position;
                _hasTargetSample = true;
                return lookAhead;
            }

            float forwardSpeed = Mathf.Max(0f, (_target.position.z - _lastTargetPosition.z) / Time.deltaTime);
            _lastTargetPosition = _target.position;
            return lookAhead + Mathf.Clamp(forwardSpeed * 0.28f, 0f, 3.25f);
        }

        private Vector3 GetShakeOffset(Quaternion cameraRotation)
        {
            if (_shakeRemaining <= 0f || _shakeAmplitude <= 0f)
            {
                return Vector3.zero;
            }

            _shakeRemaining = Mathf.Max(0f, _shakeRemaining - Time.deltaTime);
            float duration = Mathf.Max(0.01f, _shakeDuration);
            float envelope = Mathf.Clamp01(_shakeRemaining / duration);
            envelope *= envelope;

            float frequency = _tuning != null ? Mathf.Max(1f, _tuning.cameraShakeFrequency) : 26f;
            float sample = (Time.time + _shakeSeed) * frequency;
            Vector3 localOffset = new Vector3(
                Mathf.Sin(sample * 1.11f),
                Mathf.Sin(sample * 1.73f + 1.4f),
                0f) * (_shakeAmplitude * envelope);

            if (_shakeRemaining <= 0f)
            {
                _shakeAmplitude = 0f;
                _shakeDuration = 0f;
            }

            return cameraRotation * localOffset;
        }

        private void NormalizeCameraChild()
        {
            if (_camera == null || _camera.transform == transform)
            {
                return;
            }

            _camera.transform.localPosition = Vector3.zero;
            _camera.transform.localRotation = Quaternion.identity;
        }

        public static void Shake(CameraShakeCue cue)
        {
            if (_active == null)
            {
                return;
            }

            _active.GetShakeProfile(cue, out float amplitude, out float duration);
            _active.Shake(amplitude, duration);
        }

        private void GetShakeProfile(CameraShakeCue cue, out float amplitude, out float duration)
        {
            if (_tuning == null)
            {
                amplitude = cue == CameraShakeCue.BossDefeat ? 0.145f : cue == CameraShakeCue.BossHit ? 0.085f : 0.065f;
                duration = cue == CameraShakeCue.BossDefeat ? 0.24f : cue == CameraShakeCue.BossHit ? 0.16f : 0.12f;
                return;
            }

            switch (cue)
            {
                case CameraShakeCue.ObstacleBreak:
                    amplitude = _tuning.obstacleShakeAmplitude;
                    duration = _tuning.obstacleShakeDuration;
                    break;
                case CameraShakeCue.BossHit:
                    amplitude = _tuning.bossHitShakeAmplitude;
                    duration = _tuning.bossHitShakeDuration;
                    break;
                case CameraShakeCue.BossDefeat:
                    amplitude = _tuning.bossDefeatShakeAmplitude;
                    duration = _tuning.bossDefeatShakeDuration;
                    break;
                case CameraShakeCue.Victory:
                    amplitude = _tuning.victoryShakeAmplitude;
                    duration = _tuning.victoryShakeDuration;
                    break;
                default:
                    amplitude = 0f;
                    duration = 0f;
                    break;
            }
        }
    }
}
