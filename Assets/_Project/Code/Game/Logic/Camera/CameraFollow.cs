using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Transform _target;
        [SerializeField] private float _translateSpeed;
        [SerializeField] private float _rotationSpeed;

        private void LateUpdate()
        {
            if(_target == null)
                return;

            HandleTranslation();
            HandleRotation();
        }
        
        public void Follow(Transform target)
        {
            _target = target;
        }
        
        private void HandleTranslation()
        {
            var targetPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, _translateSpeed * Time.deltaTime);
        }
        
        private void HandleRotation()
        {
            var direction = _target.position - transform.position;
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
        }
    }
}