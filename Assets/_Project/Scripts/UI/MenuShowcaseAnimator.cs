using UnityEngine;

namespace ArmyRush
{
    public sealed class MenuShowcaseAnimator : MonoBehaviour
    {
        [SerializeField] private Transform[] _soldiers;
        [SerializeField] private Transform _coin;
        [SerializeField] private Transform _banner;
        [SerializeField] private float _bobAmplitude = 0.045f;
        [SerializeField] private float _bobSpeed = 3.4f;
        [SerializeField] private float _swayDegrees = 4.5f;

        private Vector3[] _basePositions;
        private Quaternion[] _baseRotations;
        private Vector3 _coinBasePosition;
        private Quaternion _coinBaseRotation;
        private Vector3 _bannerBaseScale = Vector3.one;

        private void Awake()
        {
            CacheBaseTransforms();
        }

        private void OnEnable()
        {
            CacheBaseTransforms();
        }

        private void Update()
        {
            if (_soldiers != null)
            {
                for (int i = 0; i < _soldiers.Length; i++)
                {
                    Transform soldier = _soldiers[i];
                    if (soldier == null || _basePositions == null || i >= _basePositions.Length)
                    {
                        continue;
                    }

                    float phase = Time.time * _bobSpeed + i * 0.58f;
                    float bob = Mathf.Sin(phase) * _bobAmplitude;
                    float sway = Mathf.Sin(phase * 0.72f) * _swayDegrees;
                    soldier.localPosition = _basePositions[i] + Vector3.up * bob;
                    soldier.localRotation = _baseRotations[i] * Quaternion.Euler(0f, sway, 0f);
                }
            }

            if (_coin != null)
            {
                _coin.localPosition = _coinBasePosition + Vector3.up * (Mathf.Sin(Time.time * 2.8f) * 0.035f);
                _coin.localRotation = _coinBaseRotation * Quaternion.Euler(0f, Time.time * 72f, 0f);
            }

            if (_banner != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 2.2f) * 0.025f;
                _banner.localScale = new Vector3(_bannerBaseScale.x * pulse, _bannerBaseScale.y, _bannerBaseScale.z);
            }
        }

        private void CacheBaseTransforms()
        {
            if (_soldiers != null)
            {
                _basePositions = new Vector3[_soldiers.Length];
                _baseRotations = new Quaternion[_soldiers.Length];
                for (int i = 0; i < _soldiers.Length; i++)
                {
                    if (_soldiers[i] == null)
                    {
                        continue;
                    }

                    _basePositions[i] = _soldiers[i].localPosition;
                    _baseRotations[i] = _soldiers[i].localRotation;
                }
            }

            if (_coin != null)
            {
                _coinBasePosition = _coin.localPosition;
                _coinBaseRotation = _coin.localRotation;
            }

            if (_banner != null)
            {
                _bannerBaseScale = _banner.localScale;
            }
        }
    }
}
