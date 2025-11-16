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
        
        private DiContainer _container;
        private GameLogic _gameLogic;

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }

        public override void Spawned()
        {
            _gameLogic = GameLogic.Instance;
            SetupGame();
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
            
            _container.Resolve<EnemiesProvider>().Init(EnemiesProviderState);
            _container.Resolve<PlayerProvider>().Init(PlayerProviderState);
            
            Debug.Log("InitStates end");
        }

        private void SetupGame()
        {
            if(_gameLogic == null)
                return;
            
            _gameLogic.Init(this);
        }
    }
}