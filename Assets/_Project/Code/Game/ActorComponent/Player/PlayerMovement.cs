using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
     public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private NetworkCharacterController _characterController;
        [SerializeField] private MovementStat _movementStat;
        
        public override void Spawned()
        {
            _movementStat.OnChanged += OnMovementStatChange;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _movementStat.OnChanged -= OnMovementStatChange;
        }

        public override void FixedUpdateNetwork()
        {
            if(!GetInput(out NetworkPlayerInput input))
                return;
            
            var move = new Vector3(input.Direction.x, 0f, input.Direction.y);
            _characterController.Move(move * _movementStat.State.Speed * Runner.DeltaTime);
        }
        
        private void OnMovementStatChange()
        {
            _characterController.maxSpeed = _movementStat.State.Speed;
            _characterController.rotationSpeed =  _movementStat.State.Speed;
        }
    }
}