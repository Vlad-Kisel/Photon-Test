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
        [SerializeField] private GameNetworkContext _networkGameContextPrefab;
        [SerializeField] private LocalPlayerSetupService _playerSetupService;
        
        [field: SerializeField] public GameNetworkContext NetworkGameContext { get; set; }

        private Game _game;
        private PlayerFactory _playerFactory;
        private PlayerProvider _playerProvider;
        private EnemiesProvider _enemiesProvider;

        private List<INetworkRunnerCallbacks> _callbacks =  new List<INetworkRunnerCallbacks>();
        private List<SimulationBehaviour> _simulationBehaviours = new List<SimulationBehaviour>();
        
        public PlayerConnectionService  PlayerConnectionService { get; private set; }
        public HostMigrationService HostMigrationService { get; private set; }
        
        [Inject]
        public void Construct(Game game, PlayerFactory playerFactory, PlayerProvider playerProvider,
            EnemiesProvider enemiesProvider)
        {
            _game = game;
            _playerFactory = playerFactory;
            _playerProvider = playerProvider;
            _enemiesProvider = enemiesProvider;
        }
        
        public void Init(GameNetworkContext networkContext)
        {
            NetworkGameContext = networkContext;
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

        public async Task HostMigrationResume(NetworkRunner runner) => 
            HostMigrationService.HostMigrationResume(runner);

        public void RegisterOnRunner(SimulationBehaviour behaviour) => 
            _simulationBehaviours.Add(behaviour);

        public void RegisterOnRunner(INetworkRunnerCallbacks callbacks) => 
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

        public void RegisterPlayerInstance(Player instance)
        {
            instance.GetComponent<Death>()
                .OnDie += BanPlayer;
            
            if(!_playerProvider.Players.ContainsKey(instance.PlayerId))
                _playerProvider.Register(instance.PlayerId, instance);
        }

        private void RestoreRegistrations()
        {
            _callbacks.ForEach(x => Runner.AddCallbacks(x));
            _simulationBehaviours.ForEach(x => Runner.AddGlobal(x));
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

            RegisterPlayerInstance(newPlayer);
            
            return newPlayer;
        }

        private void BanPlayer(Death death)
        {
            death.OnDie -= BanPlayer;
            
            var playerId = death.GetComponent<Player>().PlayerId;
            
            PlayerConnectionService.Ban(playerId);
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

            await NetworkGameContext.InitStates();
        }

        public void Dispose()
        {
            PlayerConnectionService?.Dispose();
        }
    }
}