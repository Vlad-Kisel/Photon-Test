using System;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class Follow : NetworkBehaviour
    {
        [SerializeField] private float stopDistance = 1f;
        [SerializeField] private NetworkCharacterController _characterController;
        [SerializeField] private Death _death;

        private Transform _target;
        private float _speed;

        public event Action OnReachedTarget;

        public bool HasTarget => _target != null;

        public void Init(float speed)
        {
            _speed = speed;
        }

        public void SetFollow(Transform target)
        {
            _target = target;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _target = null;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasTarget || _death.IsDead)
                return;

            var direction = _target.position - transform.position;
            var distance = direction.magnitude;

            if (distance <= stopDistance)
            {
                OnReachedTarget?.Invoke();
                _target = null;
                return;
            }

            Move(direction, distance);
        }

        private void Move(Vector3 direction, float distance)
        {
            var move = direction.normalized * _speed * Runner.DeltaTime;

            if (move.magnitude > distance)
                move = direction;

            _characterController.Move(move);
        }
    }
}