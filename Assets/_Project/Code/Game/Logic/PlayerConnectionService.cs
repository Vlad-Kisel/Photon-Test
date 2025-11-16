using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerConnectionService : IDisposable
    {
        private const string LogTag = "[PlayerConnectionService]";

        private readonly ICoroutineRunner _coroutineRunner;
        private readonly NetworkRunner _runner;
        private readonly GameLogic _gameLogic;

        private readonly WaitForSeconds _reconnectGracePeriod = new(60f);

        private Dictionary<int, Player> _spawnedNonActivePlayers = new();
        private Dictionary<int, Player> _activePlayers = new();
        private Dictionary<int, Coroutine> _despawnCoroutine = new();
        private Dictionary<int, PlayerConnectionData> _playerConnectionData = new();

        private Func<PlayerRef, PlayerConnectionData, Player> _createPlayer;
        private Action<Player> _destroyPlayer;
        
        public PlayerConnectionService(GameLogic gameLogic, NetworkRunner runner, ICoroutineRunner coroutineRunner,
            Func<PlayerRef, PlayerConnectionData, Player> createPlayer, Action<Player> destroyPlayer)
        {
            _gameLogic = gameLogic;
            _runner = runner;
            _coroutineRunner = coroutineRunner;
            _createPlayer = createPlayer;
            _destroyPlayer = destroyPlayer;
        }

        public void Join(PlayerRef playerRef)
        {
            var connectionData = GetConnectionData(playerRef);

            if (connectionData == null)
                return;

            var playerId = connectionData.PlayerId.GetHashCode();
            Debug.Log($"{LogTag} Join\nPlayerId:{playerId}");


            GetPlayerState(playerRef,
                out var isActive,
                out var isSpawned);

            if (isActive)
                return;

            if (isSpawned)
                Reconnect(playerRef);
            else
                SpawnNewPlayer(playerRef);
        }

        public void Left(PlayerRef playerRef)
        {
            var connectionData = GetConnectionData(playerRef);

            if (connectionData == null)
                return;

            var playerId = connectionData.PlayerId.GetHashCode();

            Debug.Log($"{LogTag} Left\nPlayerId:{playerId}");

            if (_despawnCoroutine.ContainsKey(playerId))
                return;

            _activePlayers.Remove(playerId);

            if (!_spawnedNonActivePlayers.ContainsKey(playerId))
                return;

            Despawn(playerId);
        }

        public void ConnectRequest(NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            var connectionToken = ParseConnectionToken(token);
            var playerId = connectionToken.PlayerId.GetHashCode();

            if (_gameLogic.NetworkGameContext.BlackListId.Contains(connectionToken.PlayerId.GetHashCode()))
            {
                Debug.Log($"{LogTag} ConnectRequest refuse\nPlayerId:{playerId}");
                request.Refuse();
                return;
            }

            Debug.Log($"{LogTag} ConnectRequest accept\nPlayerId:{playerId}");
            request.Accept();
        }

        public async void Ban(int playerId)
        {
            Debug.Log($"{LogTag} ban \nPlayerId:{playerId}");
            _gameLogic.NetworkGameContext.BlackListId.Add(playerId);

            var player = _activePlayers.Remove(playerId, out var p) ? p
                : _spawnedNonActivePlayers.Remove(playerId, out p) ? p
                : null;

            if (player == null)
                return;

            var banedPlayerRef = player.Object.InputAuthority;
            _destroyPlayer(player);

            if (_runner.IsServer && _runner.LocalPlayer == banedPlayerRef)
            {
                await _runner.PushHostMigrationSnapshot();
                await _runner.Shutdown();
            }
            else
                _runner.Disconnect(banedPlayerRef);
        }

        public void AddSpawnedNotActivePlayer(Player player)
        {
            _spawnedNonActivePlayers.Add(player.PlayerId, player);
            Despawn(player.PlayerId);
        }

        private PlayerConnectionData GetConnectionData(PlayerRef player)
        {
            if (_playerConnectionData.TryGetValue(player.PlayerId.GetHashCode(), out var connectionData))
                return connectionData;

            var token = _runner.GetPlayerConnectionToken(player);

            if (token == null)
                return null;

            var playerConnectionData = ParseConnectionToken(token);
            _playerConnectionData.Add(player.PlayerId, playerConnectionData);

            return playerConnectionData;
        }

        private PlayerConnectionData ParseConnectionToken(byte[] token)
        {
            var json = Encoding.UTF8.GetString(token);
            var playerConnectionData = JsonUtility.FromJson<PlayerConnectionData>(json);

            return playerConnectionData;
        }

        private void GetPlayerState(int playerId, out bool isActive, out bool isSpawned)
        {
            isActive = _activePlayers.ContainsKey(playerId);
            isSpawned = _spawnedNonActivePlayers.ContainsKey(playerId);
        }

        private void GetPlayerState(PlayerRef playerRef, out bool isActive, out bool isSpawned)
        {
            isActive = false;
            isSpawned = false;

            var connectionData = GetConnectionData(playerRef);

            if (connectionData == null)
                return;

            var playerId = connectionData.PlayerId.GetHashCode();

            GetPlayerState(playerId, out isActive, out isSpawned);
        }

        private IEnumerator DespawnPlayerAfterDisconnectTimeout(int playerId)
        {
            yield return _reconnectGracePeriod;
            _destroyPlayer(_spawnedNonActivePlayers[playerId]);
            _spawnedNonActivePlayers.Remove(playerId);
        }

        private void Despawn(int playerId)
        {
            var despawnCoroutine = _coroutineRunner.StartCoroutine(DespawnPlayerAfterDisconnectTimeout(playerId));
            _despawnCoroutine.Add(playerId, despawnCoroutine);
        }

        private void SpawnNewPlayer(PlayerRef player)
        {
            var connectionData = GetConnectionData(player);

            if (connectionData == null)
                return;

            var newPlayer = _createPlayer(player, connectionData);

            _spawnedNonActivePlayers.Add(connectionData.PlayerId.GetHashCode(), newPlayer);
            _activePlayers.Add(connectionData.PlayerId.GetHashCode(), newPlayer);
        }

        private void Reconnect(PlayerRef player)
        {
            var connectionData = GetConnectionData(player);

            if (connectionData == null)
                return;

            var playerId = connectionData.PlayerId.GetHashCode();
            Debug.Log($"{LogTag} Reconnect\nPlayerId:{playerId}");

            if (_despawnCoroutine.TryGetValue(playerId, out var coroutine))
                _coroutineRunner.StopCoroutine(coroutine);

            _spawnedNonActivePlayers[playerId].Object
                .AssignInputAuthority(player);
        }

        public void Dispose()
        {
            foreach (var coroutine in _despawnCoroutine.Values)
                _coroutineRunner.StopCoroutine(coroutine);

            _spawnedNonActivePlayers.Clear();
            _activePlayers.Clear();
            _despawnCoroutine.Clear();
            _playerConnectionData.Clear();
        }
    }
}