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

        private bool _used;
        private Color _baseColor;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            if (_panelRenderer != null)
            {
                _baseColor = _panelRenderer.sharedMaterial != null ? _panelRenderer.sharedMaterial.color : Color.white;
            }
        }

        public void Configure(GateOperation operation, int value)
        {
            _operation = operation;
            _value = value;
            _used = false;
            UpdateVisuals();
        }

        private void OnEnable()
        {
            _used = false;
            UpdateVisuals();
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
                audio.Play(positive ? AudioCue.GatePositive : AudioCue.GateNegative);
            }
            if (ServiceLocator.TryGet(out HapticsService haptics))
            {
                haptics.Play(positive ? HapticCue.Light : HapticCue.Warning);
            }

            StartCoroutine(AnimateUsed());
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
            }

            if (_panelRenderer != null)
            {
                Color color = IsPositive() ? new Color(0.05f, 0.95f, 0.72f, 0.82f) : new Color(1f, 0.24f, 0.12f, 0.82f);
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                _panelRenderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", color);
                block.SetColor("_Color", color);
                _panelRenderer.SetPropertyBlock(block);
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

        private System.Collections.IEnumerator AnimateUsed()
        {
            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(t / 0.18f * Mathf.PI) * 0.18f;
                transform.localScale = startScale * pulse;
                yield return null;
            }
            transform.localScale = startScale;

            if (_panelRenderer != null && _panelRenderer.material != null)
            {
                Color color = _baseColor;
                color.a = 0.35f;
                _panelRenderer.material.color = color;
            }
        }
    }
}
