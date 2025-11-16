using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class GameLogic : SimulationBehaviour, IPlayerJoined, IPlayerLeft, ICoroutineRunner, IDisposable
    {
        public static GameLogic Instance {get; private set;}
        
        [SerializeField] private GameNetworkContext _networkGameContextPrefab;
        [SerializeField] private LocalPlayerSetupService _playerSetupService;
        
        [field: SerializeField] public GameNetworkContext NetworkGameContext { get; set; }

        private Game _game;
        private PlayerFactory _playerFactory;
        private PlayerProvider _playerProvider;
        private EnemiesProvider _enemiesProvider;
        private DiContainer _diContainer;

        private List<INetworkRunnerCallbacks> _callbacks =  new List<INetworkRunnerCallbacks>();
        private List<SimulationBehaviour> _simulationBehaviours = new List<SimulationBehaviour>();
        
        public PlayerConnectionService  PlayerConnectionService { get; private set; }
        public HostMigrationService HostMigrationService { get; private set; }
        
        [Inject]
        public void Construct(Game game, PlayerFactory playerFactory, PlayerProvider playerProvider,
            DiContainer diContainer, EnemiesProvider enemiesProvider)
        {
            _game = game;
            _playerFactory = playerFactory;
            _playerProvider = playerProvider;
            _enemiesProvider = enemiesProvider;
            _diContainer = diContainer;
            
            Instance = this;
        }
        
        public void Init(GameNetworkContext networkContext)
        {
            NetworkGameContext = networkContext;
        }

        public async Task HostMigrationResume(NetworkRunner runner) => 
            HostMigrationService.HostMigrationResume(runner);

        private void RestoreRegistrations()
        {
            _callbacks.ForEach(x => Runner.AddCallbacks(x));
            _simulationBehaviours.ForEach(x => Runner.AddGlobal(x));
        }
        
        public void Register(SimulationBehaviour behaviour) => 
            _simulationBehaviours.Add(behaviour);

        public void Register(INetworkRunnerCallbacks callbacks) => 
            _callbacks.Add(callbacks);

        public async Task  PrepareGame(NetworkRunner runner)
        {
            runner.AddGlobal(this);
            await InitNetworkServicesStates(runner);
            
            PlayerConnectionService?.Dispose();

            PlayerConnectionService = new PlayerConnectionService(this, _game.Runner, this, CreatePlayer, DestroyPlayer);
            HostMigrationService = new HostMigrationService(this, _enemiesProvider);

            RestoreRegistrations();
            
            _playerSetupService.Init();
        }

        private void DestroyPlayer(Player player)
        {
            if(!Runner.IsServer)
                return;
            
            _playerProvider.Unregister(player.PlayerId);
            
            Runner.Despawn(player.Object);
        }

        private Player CreatePlayer(PlayerRef playerRef, PlayerConnectionData connectionData)
        {
            if(!Runner.IsServer)
                return null;
            
            var newPlayer = _playerFactory.Create(Vector3.up, Quaternion.identity, connectionData, connectionData.PlayerId.GetHashCode());
            newPlayer.Object.AssignInputAuthority(playerRef);

            Register(newPlayer);
            
            return newPlayer;
        }

        public void Register(Player instance)
        {
            instance.GetComponent<Death>()
                .OnDie += BanPlayer;
            
            if(!_playerProvider.Players.ContainsKey(instance.PlayerId))
                _playerProvider.Register(instance.PlayerId, instance);
        }
        
        private void BanPlayer(Death death)
        {
            death.OnDie -= BanPlayer;
            
            var playerId = death.GetComponent<Player>().PlayerId;
            
            PlayerConnectionService.Ban(playerId);
        }

        public void PlayerLeft(PlayerRef playerRef)
        {
            if(!_game.Runner.IsServer || NetworkGameContext == null)
                return;
            
            PlayerConnectionService.Left(playerRef);
        }

        public void PlayerJoined(PlayerRef playerRef)
        {
            if(!_game.Runner.IsServer ||  NetworkGameContext == null)
                return;
            
            PlayerConnectionService.Join(playerRef);
        }

        private async Task InitNetworkServicesStates(NetworkRunner runner)
        {
            if (NetworkGameContext == null)
            {
                if (runner.CanSpawn && runner.IsServer)
                {
                    await runner.SpawnAsync(_networkGameContextPrefab);
                }
                else
                {
                    while (NetworkGameContext == null)
                        await Task.Yield();
                }
            }

            while (NetworkGameContext == null || NetworkGameContext.Object == null ||
                   !NetworkGameContext.Object.IsValid)
                await Task.Yield();

            _diContainer.Inject(NetworkGameContext);
            await NetworkGameContext.InitStates();
        }

        
        public void Dispose()
        {
            PlayerConnectionService?.Dispose();
            
            Instance = null;
        }
    }
}