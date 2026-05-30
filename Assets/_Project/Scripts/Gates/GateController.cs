using UnityEngine;

namespace ArmyRush
{
    [RequireComponent(typeof(Collider))]
    public sealed class GateController : MonoBehaviour
    {
        [SerializeField] private GateOperation _operation;
        [SerializeField] private int _value = 10;
        [SerializeField] private TextMesh _label;
        [SerializeField] private Renderer _panelRenderer;
        [SerializeField] private ParticleSystem _burst;
        [SerializeField] private float _activationHalfWidth = 1.05f;
        [SerializeField] private float _idleGlowSpeed = 4.2f;
        [SerializeField] private float _idleGlowStrength = 0.18f;
        [SerializeField] private float _emissionStrength = 0.85f;

        private bool _used;
        private Color _baseColor;
        private Vector3 _baseScale;
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _animationRoutine;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            _baseScale = transform.localScale;
            _propertyBlock = new MaterialPropertyBlock();
            if (_panelRenderer != null)
            {
                _baseColor = _panelRenderer.sharedMaterial != null ? _panelRenderer.sharedMaterial.color : Color.white;
            }
        }

        private void Update()
        {
            if (!_used)
            {
                UpdateIdleGlow();
            }
        }

        public void Configure(GateOperation operation, int value)
        {
            _operation = operation;
            _value = value;
            _used = false;
            ResetAnimationState();
            UpdateVisuals();
        }

        private void OnEnable()
        {
            _used = false;
            ResetAnimationState();
            UpdateVisuals();
        }

        private void OnDisable()
        {
            StopActivationAnimation();
            transform.localScale = _baseScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_used)
            {
                return;
            }

            CrowdManager crowd = other.GetComponentInParent<CrowdManager>();
            if (crowd == null)
            {
                return;
            }

            if (Mathf.Abs(crowd.transform.position.x - transform.position.x) > _activationHalfWidth)
            {
                return;
            }

            _used = true;
            Apply(crowd);
            VfxManager.SpawnFloatingText(GetLabel(), crowd.transform.position + Vector3.up * 2.45f, IsPositive() ? new Color(0.2f, 1f, 0.65f) : new Color(1f, 0.25f, 0.15f));
            if (_burst != null)
            {
                _burst.Play();
            }

            bool positive = IsPositive();
            VfxManager.Spawn(positive ? VfxCue.GatePositive : VfxCue.GateNegative, transform.position + Vector3.up * 1.2f);
            if (ServiceLocator.TryGet(out AudioService audio))
            {
                audio.Play(AudioCue.GatePass);
                audio.Play(positive ? AudioCue.GatePositive : AudioCue.GateNegative);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(positive ? HapticCue.Light : HapticCue.Warning);
            }

            StopActivationAnimation();
            _animationRoutine = StartCoroutine(AnimateUsed(positive));
        }

        private void Apply(CrowdManager crowd)
        {
            switch (_operation)
            {
                case GateOperation.Add:
                    crowd.Add(_value);
                    break;
                case GateOperation.Subtract:
                    crowd.Remove(_value);
                    break;
                case GateOperation.Multiply:
                    crowd.Multiply(_value);
                    break;
                case GateOperation.Divide:
                    crowd.Divide(_value);
                    break;
            }
        }

        private void UpdateVisuals()
        {
            if (_label != null)
            {
                _label.text = GetLabel();
                _label.color = Color.white;
                WorldTextGuard.Clamp(_label);
            }

            if (_panelRenderer != null)
            {
                Color color = IsPositive() ? new Color(0.05f, 0.95f, 0.72f, 0.82f) : new Color(1f, 0.24f, 0.12f, 0.82f);
                SetPanelColor(color);
                _baseColor = color;
            }
        }

        private string GetLabel()
        {
            switch (_operation)
            {
                case GateOperation.Add:
                    return "+" + _value;
                case GateOperation.Subtract:
                    return "-" + _value;
                case GateOperation.Multiply:
                    return "x" + _value;
                case GateOperation.Divide:
                    return "/" + _value;
                default:
                    return _operation.ToString();
            }
        }

        private bool IsPositive()
        {
            return _operation == GateOperation.Add || _operation == GateOperation.Multiply || _operation == GateOperation.DamageBoost || _operation == GateOperation.FireRateBoost || _operation == GateOperation.CoinBoost;
        }

        private void SetPanelColor(Color color)
        {
            if (_panelRenderer == null)
            {
                return;
            }

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            _panelRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            float emission = _emissionStrength * Mathf.Clamp01(color.a);
            _propertyBlock.SetColor("_EmissionColor", new Color(color.r, color.g, color.b, 1f) * emission);
            _panelRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void UpdateIdleGlow()
        {
            if (_panelRenderer == null)
            {
                return;
            }

            float phase = Time.time * _idleGlowSpeed + transform.position.z * 0.11f;
            float pulse = 0.5f + Mathf.Sin(phase) * 0.5f;
            Color glowColor = Color.Lerp(_baseColor, Color.white, _idleGlowStrength * pulse);
            glowColor.a = _baseColor.a;
            SetPanelColor(glowColor);

            if (_label != null)
            {
                _label.color = Color.Lerp(Color.white, IsPositive() ? new Color(0.72f, 1f, 0.88f) : new Color(1f, 0.78f, 0.62f), pulse * 0.24f);
            }
        }

        private void ResetAnimationState()
        {
            StopActivationAnimation();
            transform.localScale = _baseScale;
            if (_label != null)
            {
                _label.color = Color.white;
            }
        }

        private void StopActivationAnimation()
        {
            if (_animationRoutine == null)
            {
                return;
            }

            StopCoroutine(_animationRoutine);
            _animationRoutine = null;
        }

        private System.Collections.IEnumerator AnimateUsed(bool positive)
        {
            const float duration = 0.24f;
            float t = 0f;
            Vector3 startScale = _baseScale;
            Color hitColor = positive ? Color.Lerp(_baseColor, Color.white, 0.55f) : new Color(1f, 0.62f, 0.24f, _baseColor.a);
            Color usedColor = _baseColor;
            usedColor.a = 0.35f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);
                float eased = EaseOutCubic(normalized);
                float pulse = 1f + Mathf.Sin(normalized * Mathf.PI) * 0.14f;
                transform.localScale = startScale * pulse;
                SetPanelColor(Color.Lerp(hitColor, usedColor, eased));
                if (_label != null)
                {
                    Color labelColor = Color.white;
                    labelColor.a = Mathf.Lerp(1f, 0.45f, eased);
                    _label.color = labelColor;
                }
                yield return null;
            }

            transform.localScale = startScale;
            SetPanelColor(usedColor);
            if (_label != null)
            {
                Color labelColor = Color.white;
                labelColor.a = 0.45f;
                _label.color = labelColor;
            }
            _animationRoutine = null;
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - value;
            return 1f - inverse * inverse * inverse;
        }
    }
}
