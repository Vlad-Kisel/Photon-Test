using System;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerFactory : MonoBehaviour
    {
        [SerializeField] private NetworkPrefabRef playerPrefab;
        
        private PlayerProvider _playerProvider;
        private Game _game;
        private DiContainer _diContainer;

        [Inject]
        public void Construct(Game game, PlayerProvider playerProvider, DiContainer diContainer)
        {
            _game = game;
            _playerProvider = playerProvider;
            _diContainer = diContainer;
            
            _playerProvider.PlayerAdded += InitializePlayer;
        }

        private void OnDestroy()
        {
            _playerProvider.PlayerAdded -= InitializePlayer;
        }

        private void InitializePlayer(Player player)
        {
            player.GetComponentInChildren<HealthView>()
                .Init(player.Stats.Health);
            
            _diContainer.InjectGameObject(player.gameObject);
        }
        
        public Player Create(Vector3 position, Quaternion rotation, PlayerConnectionData playerConnectionData, int playerId)
        {
            if(!_game.Runner.IsServer)
                throw new Exception();
            
            var playerObject = _game.Runner.Spawn(playerPrefab, Vector3.up, Quaternion.identity);
            
            var state = playerObject.GetComponent<PlayerGameState>();
            state.HeapId = playerConnectionData.SkinId;

            var player = playerObject.GetComponent<Player>();
            player.Init(playerId);
            
            player.Stats.Movement.Init(Constants.BasePlayerMovementStat);
            player.Stats.Attack.Init(Constants.BasePlayerAttackStat);
            player.Stats.Health.Init(Constants.BasePlayerHealthStat);
            player.Stats.Level.Init(Constants.BasePlayerLevelStat);
            
            _playerProvider.Register(playerId, player);
            
            InitializePlayer(player);
            
            return player;
        }
    }
}