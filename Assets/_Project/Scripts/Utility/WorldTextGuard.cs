using UnityEngine;

namespace ArmyRush
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMesh))]
    public sealed class WorldTextGuard : MonoBehaviour
    {
        private const int DefaultMaxFontSize = 58;
        private const float DefaultMaxCharacterSize = 0.075f;
        private const float MinimumCharacterSize = 0.025f;
        private const float MaxLocalScaleAxis = 1f;

        [SerializeField] private TextMesh _textMesh;

        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMesh>();
            }
            Clamp(_textMesh);
        }

        private void OnEnable()
        {
            Clamp(_textMesh);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMesh>();
            }
            Clamp(_textMesh);
        }
#endif

        public static void ClampSceneText()
        {
            TextMesh[] labels = Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < labels.Length; i++)
            {
                Clamp(labels[i]);
            }
        }

        public static void Clamp(TextMesh textMesh)
        {
            if (textMesh == null)
            {
                return;
            }

            GetLimits(textMesh.name, out int maxFontSize, out float maxCharacterSize);
            textMesh.fontSize = Mathf.Clamp(textMesh.fontSize, 24, maxFontSize);
            textMesh.characterSize = Mathf.Clamp(textMesh.characterSize, MinimumCharacterSize, maxCharacterSize);

            Transform textTransform = textMesh.transform;
            Vector3 localScale = textTransform.localScale;
            float largestAxis = Mathf.Max(Mathf.Abs(localScale.x), Mathf.Abs(localScale.y), Mathf.Abs(localScale.z));
            if (largestAxis > MaxLocalScaleAxis)
            {
                textTransform.localScale = localScale * (MaxLocalScaleAxis / largestAxis);
            }
        }

        public static void Ensure(TextMesh textMesh)
        {
            if (textMesh == null)
            {
                return;
            }

            Clamp(textMesh);
            if (textMesh.GetComponent<WorldTextGuard>() == null)
            {
                textMesh.gameObject.AddComponent<WorldTextGuard>();
            }
        }

        private static void GetLimits(string labelName, out int maxFontSize, out float maxCharacterSize)
        {
            maxFontSize = DefaultMaxFontSize;
            maxCharacterSize = DefaultMaxCharacterSize;

            if (string.IsNullOrEmpty(labelName))
            {
                return;
            }

            if (labelName.Contains("CrowdCount"))
            {
                maxFontSize = 52;
                maxCharacterSize = 0.062f;
                return;
            }

            if (labelName.Contains("TextLabel"))
            {
                maxFontSize = 54;
                maxCharacterSize = 0.068f;
                return;
            }

            if (labelName.Contains("GateHintLabel"))
            {
                maxFontSize = 50;
                maxCharacterSize = 0.062f;
                return;
            }

            if (labelName.Contains("CountLabel"))
            {
                maxFontSize = 52;
                maxCharacterSize = 0.066f;
                return;
            }

            if (labelName.Contains("BossHealth"))
            {
                maxFontSize = 50;
                maxCharacterSize = 0.062f;
                return;
            }

            if (labelName.Contains("Health"))
            {
                maxFontSize = 48;
                maxCharacterSize = 0.058f;
                return;
            }

            if (labelName.Contains("Reward") || labelName.Contains("Finish") || labelName.Contains("Claim"))
            {
                maxFontSize = 52;
                maxCharacterSize = 0.066f;
            }
        }
    }
}
