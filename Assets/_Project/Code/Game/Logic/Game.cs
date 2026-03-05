using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class Game : MonoBehaviour, INetworkRunnerCallbacks
    {
        private const string LogTag = "[Game]";
        
        private readonly SceneRef _gameSceneRef = SceneRef.FromIndex((int)GameScene.Game);
        
        [SerializeField] private NetworkRunner runnerPrefab;

        private GameServicesWrapper _gameServicesWrapper;
        
        private CancellationTokenSource _loadSceneCancellationTokenSource = new CancellationTokenSource();
        private byte[] _playerConnectionToken;
        
        public event Action OnGameStart;
        
        public NetworkRunner Runner { get; private set; }
        
        [Inject]
        public void Construct(GameServicesWrapper gameServicesWrapper)
        {
            _gameServicesWrapper = gameServicesWrapper;
        }

        public async void StartGame(string sessionName)
        {
            Debug.Log($"{LogTag} Starting game");
            
            CreateRunner();
            Runner.AddCallbacks(this);

            DontDestroyOnLoad(Runner);

            var playerConnectionData = new PlayerConnectionData()
            {
#if UNITY_EDITOR
                SkinId = "Hat_1",
                //PlayerId = "1"
                //PlayerId = Guid.NewGuid().ToString()
#else
                SkinId = "Hat_2",
                //PlayerId = "2"
                //PlayerId = Guid.NewGuid().ToString()
#endif
                PlayerId = Guid.NewGuid().ToString()
            };

            _playerConnectionToken = Encoding.UTF8.GetBytes(JsonUtility.ToJson(playerConnectionData));

            var sceneManager = Runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = sessionName,
                SceneManager = sceneManager,
                ConnectionToken = _playerConnectionToken,
                IsOpen = true,
                IsVisible = true
            });

            Debug.Log($"Game started {Runner.SceneManager.MainRunnerScene.path}");
            await LoadGameScene();
            
            Debug.Log($"Game LoadGameScene {Runner.SceneManager.MainRunnerScene.path}");
            await _gameServicesWrapper.GameLogic.PrepareGame(Runner);
            _gameServicesWrapper.GameLogic.RegisterOnRunner(this);
            
            Debug.Log($"Game start done {Runner.SceneManager.MainRunnerScene.path}");
            foreach (var playerRef in Runner.ActivePlayers)
                _gameServicesWrapper.GameLogic.PlayerJoined(playerRef);

            Debug.Log($"Game start {Runner.SceneManager.MainRunnerScene.path}");
            OnGameStart?.Invoke();
        }

        public void EndGame()
        {
            Debug.Log($"{LogTag} End game");

            Runner.Shutdown();
            Destroy(Runner);

            SceneManager.LoadScene(nameof(GameScene.Lobby));
        }

        private async void HostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);
            
            CreateRunner();
            
            var sceneManager = Runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            var networkSceneInfo = new NetworkSceneInfo();
            networkSceneInfo.AddSceneRef(_gameSceneRef);
            var result = await Runner.StartGame(new StartGameArgs()
            {
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = HostMigrationResume,
                ConnectionToken = _playerConnectionToken,
                Scene = networkSceneInfo,
                SceneManager = sceneManager
            });
            
            if(!result.Ok)
                EndGame();
            
            if(Runner.IsServer)
                return;
            
            await LoadGameScene();

            await _gameServicesWrapper.GameLogic.PrepareGame(Runner);
        }

        private void CreateRunner()
        {
            Runner = Instantiate(runnerPrefab);
            
            _loadSceneCancellationTokenSource.Cancel();
            _loadSceneCancellationTokenSource.Dispose();
            _loadSceneCancellationTokenSource = new CancellationTokenSource();
        }

        private async void HostMigrationResume(NetworkRunner runner)
        {
            await LoadGameScene();
            
            await _gameServicesWrapper.GameLogic.PrepareGame(Runner);
            
            await _gameServicesWrapper.GameLogic.HostMigrationResume(runner);
            
            foreach (var playerRef in Runner.ActivePlayers)
                _gameServicesWrapper.GameLogic.PlayerJoined(playerRef);

            OnGameStart?.Invoke();
        }

        private async Task LoadGameScene()
        {
            var cancellationToken = _loadSceneCancellationTokenSource.Token;
            
            if (!Runner.IsServer)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if(Runner.SceneManager.MainRunnerScene.name == nameof(GameScene.Game))
                        break;
                    
                    await Task.Yield();
                }
        
                Debug.Log($"Game scene {(cancellationToken.IsCancellationRequested ? "cancelled" : "loaded")} on client");
                return;
            }
    
            if (Runner.SceneManager.MainRunnerScene.name == nameof(GameScene.Game))
            {
                Debug.Log("Game scene already loaded on server");
                return;
            }
            
            if (cancellationToken.IsCancellationRequested)
            {
                Debug.Log("Loading cancelled before start");
                return;
            }
    
            Debug.Log("Server loading game scene...");
            await Runner.LoadScene(_gameSceneRef);
    
            Debug.Log("Game scene loaded on server");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if(shutdownReason == ShutdownReason.HostMigration)
                return;
            
            EndGame();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.LogError($"Disconnected from server {reason}");
            EndGame();
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
            _gameServicesWrapper.GameLogic.PlayerConnectionService.ConnectRequest(request, token);
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.LogError($"ConnectFailed {reason}");
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            HostMigration(runner, hostMigrationToken);
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }
    }
}