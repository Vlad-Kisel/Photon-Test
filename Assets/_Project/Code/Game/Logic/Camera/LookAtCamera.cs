using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private bool lockY = true;

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_camera == null)
                return;

            var direction = transform.position - _camera.transform.position;

            if (lockY)
                direction.y = 0f;

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}