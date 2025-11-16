using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class NetworkInputSender : SimulationBehaviour, INetworkRunnerCallbacks, IBeforeUpdate
    {
        private NetworkPlayerInput _networkPlayerInput;
        private NetworkPlayerInput _accumulatedInput;
        private bool _resetInput;
        
        private InputService _inputService;
        
        [Inject]
        public void Construct(GameLogic gameLogic, InputService inputService)
        {
            gameLogic.Register((INetworkRunnerCallbacks)this);
            gameLogic.Register((SimulationBehaviour)this);
            
            _inputService = inputService;
        }
        
        public void BeforeUpdate()
        {
            if (_resetInput)
            {
                _resetInput = false;
                _accumulatedInput = default;
            }

            AccumulateInput();
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _accumulatedInput.Direction.Normalize();
            input.Set(_accumulatedInput);
            _resetInput = true;
        }
        
        private void AccumulateInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            Vector2 moveDirection = _inputService.Direction;

#if UNITY_EDITOR
            if (keyboard.wKey.isPressed)
                moveDirection += Vector2.up;
            if (keyboard.sKey.isPressed)
                moveDirection += Vector2.down;
            if (keyboard.aKey.isPressed)
                moveDirection += Vector2.left;
            if (keyboard.dKey.isPressed)
                moveDirection += Vector2.right;
#endif

            _accumulatedInput.Direction += moveDirection;
        }
        
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player){ }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player){ }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason){ }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason){ }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){ }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){ }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message){ }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){ }
        public void OnConnectedToServer(NetworkRunner runner){ }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList){ }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data){ }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){ }
        public void OnSceneLoadDone(NetworkRunner runner){ }
        public void OnSceneLoadStart(NetworkRunner runner){ }
    }
}