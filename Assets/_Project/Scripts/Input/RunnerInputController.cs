using UnityEngine;

namespace ArmyRush
{
    public sealed class RunnerInputController : MonoBehaviour
    {
        private bool _dragging;
        private int _touchId = -1;
        private Vector2 _lastPosition;
        private float _pendingDeltaX;
        private bool _startPressed;

        public bool IsDragging => _dragging;

        public bool ConsumeStartPressed()
        {
            bool value = _startPressed;
            _startPressed = false;
            return value;
        }

        public float ConsumeDeltaX()
        {
            float value = _pendingDeltaX;
            _pendingDeltaX = 0f;
            return value;
        }

        private void Update()
        {
            ReadTouch();
            ReadMouse();
#if UNITY_EDITOR
            ReadKeyboard();
#endif
        }

        private void ReadTouch()
        {
            if (Input.touchCount <= 0)
            {
                if (_touchId != -1)
                {
                    EndDrag();
                }
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (_touchId != -1 && touch.fingerId != _touchId)
                {
                    continue;
                }

                if (touch.phase == TouchPhase.Began)
                {
                    BeginDrag(touch.position, touch.fingerId);
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    ContinueDrag(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    EndDrag();
                }

                break;
            }
        }

        private void ReadMouse()
        {
            if (Input.touchCount > 0)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                BeginDrag(Input.mousePosition, -1);
            }
            else if (Input.GetMouseButton(0) && _dragging)
            {
                ContinueDrag(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && _dragging)
            {
                EndDrag();
            }
        }

        private void ReadKeyboard()
        {
            float keyboard = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                keyboard -= 1f;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                keyboard += 1f;
            }

            if (!Mathf.Approximately(keyboard, 0f))
            {
                _pendingDeltaX += keyboard * 900f * Time.unscaledDeltaTime;
                _startPressed = true;
            }
        }

        private void BeginDrag(Vector2 position, int touchId)
        {
            _dragging = true;
            _touchId = touchId;
            _lastPosition = position;
            _startPressed = true;
        }

        private void ContinueDrag(Vector2 position)
        {
            if (!_dragging)
            {
                BeginDrag(position, -1);
                return;
            }

            _pendingDeltaX += position.x - _lastPosition.x;
            _lastPosition = position;
        }

        private void EndDrag()
        {
            _dragging = false;
            _touchId = -1;
        }
    }
}
