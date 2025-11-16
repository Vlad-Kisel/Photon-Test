using System.Linq;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class HostMigrationService
    {
        private readonly MigrationBucket<Player> _players = new();
        private readonly MigrationBucket<Enemy> _enemies = new();
        private readonly MigrationBucket<ItemDrop> _drops = new();
        private readonly MigrationBucket<GameNetworkContext> _gameContext = new();

        private readonly GameLogic _gameLogic;
        private readonly EnemiesProvider _enemiesProvider;
        
        public HostMigrationService(GameLogic gameLogic, EnemiesProvider enemiesProvider)
        {
            _gameLogic = gameLogic;
            _enemiesProvider = enemiesProvider;
        }

        public void HostMigrationResume(NetworkRunner runner)
        {
            Collect(runner);
            Resume(runner);
            Register();
            
            Clear();
        }

        private void Clear()
        {
            _players.Clear();
            _enemies.Clear();
            _drops.Clear();
        }

        private void Collect(NetworkRunner runner)
        {
            var snapshotObjects = runner.GetResumeSnapshotNetworkObjects();
            Debug.LogError(snapshotObjects.Count());
            foreach (var snapshot in snapshotObjects)
            {
                _players.Collect(snapshot);
                _enemies.Collect(snapshot);
                _drops.Collect(snapshot);
                
                _gameContext.Collect(snapshot);
            }
        }

        private void Resume(NetworkRunner runner)
        {
            ResumeGameContext();
            
            _players.Resume(runner, PlayerFilter);
            _enemies.Resume(runner);
            _drops.Resume(runner);
        }

        private bool PlayerFilter(Player player)
        {   
            return !_gameLogic.NetworkGameContext.BlackListId.Contains(player.PlayerId);
        }

        private void ResumeGameContext()
        {
            var snapshotObject = _gameContext.Snapshots.First();
            var blackList = snapshotObject.GetComponent<GameNetworkContext>().BlackListId;
            foreach (var banPlayerId in blackList)
                _gameLogic.NetworkGameContext.BlackListId.Add(banPlayerId);
        }

        private void Register()
        {
            foreach (var player in _players.Spawned)
                RegisterPlayer(player);

            foreach (var enemy in _enemies.Spawned)
                RegisterEnemy(enemy);
        }

        private void RegisterEnemy(Enemy enemy)
        {
            _enemiesProvider.Register(enemy);
        }

        private void RegisterPlayer(Player player)
        {
            _gameLogic.PlayerConnectionService.AddSpawnedNotActivePlayer(player);
            _gameLogic.Register(player);
        }
    }
}