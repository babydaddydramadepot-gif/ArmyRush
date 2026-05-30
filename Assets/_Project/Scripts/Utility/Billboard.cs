using UnityEngine;

namespace ArmyRush
{
    public sealed class Billboard : MonoBehaviour
    {
        private static Camera _mainCamera;
        private Camera _camera;

        public static Camera SharedCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }

                return _mainCamera;
            }
        }

        public static void RegisterCamera(Camera camera)
        {
            if (camera != null)
            {
                _mainCamera = camera;
            }
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = SharedCamera;
            }

            if (_camera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position, Vector3.up);
            }
        }
    }
}
