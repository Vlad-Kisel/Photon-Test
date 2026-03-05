using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class GameNetworkContext : NetworkBehaviour
    {
        [SerializeField, Networked, Capacity(Constants.MaxPlayerCount)] public NetworkLinkedList<int> BlackListId { get; }
        
        [SerializeField, Networked] public EnemiesProviderState EnemiesProviderState {get; private  set; }
        [SerializeField, Networked] public PlayerProviderState PlayerProviderState {get; private set; }
        
        private GameLogic _gameLogic;
        private EnemiesProvider _enemiesProvider;
        private PlayerProvider _playerProvider;

        [Inject]
        public void Construct(GameLogic gameLogic, EnemiesProvider enemiesProvider, PlayerProvider playerProvider)
        {
            _gameLogic = gameLogic;
            _enemiesProvider = enemiesProvider;
            _playerProvider = playerProvider;
        }

        public override void Spawned()
        {
            SetupGameLogic();
        }
        
        public async Task InitStates()
        {
            Debug.Log("InitStates start");

            while (!Object.IsValid)
                await Task.Yield();
            
            while (!EnemiesProviderState.Object.IsValid)
                await Task.Yield();
            
            while (!PlayerProviderState.Object.IsValid)
                await Task.Yield();
            
            _enemiesProvider.Init(EnemiesProviderState);
            _playerProvider.Init(PlayerProviderState);
            
            Debug.Log("InitStates end");
        }

        private void SetupGameLogic()
        {
            _gameLogic.Init(this);
        }
    }
}