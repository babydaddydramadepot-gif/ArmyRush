using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
#if ENABLE_INPUT_SYSTEM
            ReadInputSystemTouch();
            ReadInputSystemMouse();
#if UNITY_EDITOR
            ReadInputSystemKeyboard();
#endif
#else
            ReadTouch();
            ReadMouse();
#if UNITY_EDITOR
            ReadKeyboard();
#endif
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void ReadInputSystemTouch()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                if (_touchId != -1)
                {
                    EndDrag();
                }
                return;
            }

            bool matchedActiveTouch = false;
            for (int i = 0; i < touchscreen.touches.Count; i++)
            {
                UnityEngine.InputSystem.Controls.TouchControl touch = touchscreen.touches[i];
                int touchId = touch.touchId.ReadValue();
                if (_touchId != -1 && touchId != _touchId)
                {
                    continue;
                }

                UnityEngine.InputSystem.TouchPhase phase = touch.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    BeginDrag(touch.position.ReadValue(), touchId);
                    matchedActiveTouch = true;
                    break;
                }

                if (phase == UnityEngine.InputSystem.TouchPhase.Moved || phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    Vector2 position = touch.position.ReadValue();
                    if (_dragging)
                    {
                        ContinueDrag(position);
                    }
                    else
                    {
                        BeginDrag(position, touchId);
                    }
                    matchedActiveTouch = true;
                    break;
                }

                if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    EndDrag();
                    matchedActiveTouch = true;
                    break;
                }
            }

            if (!matchedActiveTouch && _touchId != -1)
            {
                EndDrag();
            }
        }

        private void ReadInputSystemMouse()
        {
            if (_touchId != -1)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                BeginDrag(mouse.position.ReadValue(), -1);
            }
            else if (mouse.leftButton.isPressed && _dragging)
            {
                ContinueDrag(mouse.position.ReadValue());
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _dragging)
            {
                EndDrag();
            }
        }

#if UNITY_EDITOR
        private void ReadInputSystemKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            float horizontal = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            if (!Mathf.Approximately(horizontal, 0f))
            {
                _pendingDeltaX += horizontal * 900f * Time.unscaledDeltaTime;
                _startPressed = true;
            }
        }
#endif
#else
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

#if UNITY_EDITOR
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
#endif
#endif

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
