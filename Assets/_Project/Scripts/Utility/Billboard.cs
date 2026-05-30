using UnityEngine;

namespace ArmyRush
{
    public sealed class Billboard : MonoBehaviour
    {
        private static Camera _mainCamera;
        private Camera _camera;

        private void LateUpdate()
        {
            if (_camera == null)
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                _camera = _mainCamera;
            }

            if (_camera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position, Vector3.up);
            }
        }
    }
}
