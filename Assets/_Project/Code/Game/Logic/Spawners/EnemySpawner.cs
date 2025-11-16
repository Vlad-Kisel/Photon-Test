using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<PlayerVision> _playerVisions = new List<PlayerVision>();
        
        private Game _game;
        private PlayerProvider  _playerProvider;
        private EnemyFactory _enemyFactory;
        private EnemiesProvider  _enemiesProvider;
        
        private Coroutine _spawnCoroutine;

        [Inject]
        public void Construct(PlayerProvider playerProvider, EnemyFactory enemyFactory, Game game, EnemiesProvider enemiesProvider)
        {
            _playerProvider = playerProvider;
            _enemyFactory = enemyFactory;
            _enemiesProvider = enemiesProvider;
            
            _game = game;
            
            _game.OnGameStart += SpawnEnemies;
            _playerProvider.PlayersUpdated += UpdatePlayerVisions;
        }

        private void OnDestroy()
        {
            if(_game != null)
                _game.OnGameStart -= SpawnEnemies;
        }

        private void UpdatePlayerVisions()
        {
            _playerVisions = _playerProvider.Players.Select(x => x.Value.GetComponent<PlayerVision>()).ToList();
        }

        private void SpawnEnemies()
        {
            if (!_game.Runner.IsServer)
                return;
            
            UpdatePlayerVisions();
            
            _spawnCoroutine ??= StartCoroutine(AutoSpawn());            
        }
        
        private IEnumerator AutoSpawn()
        {
            while (true)
            { 
                yield return new WaitForSeconds(0.5f);
                
                if (!TryGetPointOnPlayerBoundary(_playerVisions, out var spawnPoint) || _enemiesProvider.Enemies.Count >= Constants.MaxEnemyCount)
                    continue;
                
                var enemy = _enemyFactory.Create(spawnPoint, Quaternion.identity);
                enemy.transform.parent = transform;
            }
        }
        
        private bool TryGetPointOnPlayerBoundary(List<PlayerVision> visionZones, out Vector3 spawnPoint, int maxAttempts = 50)
        {
            spawnPoint = Vector3.zero;

            if (visionZones == null || visionZones.Count == 0)
                return false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var visionZone = GetRandomVisionZone(visionZones);
                
                if(visionZone == null)
                    return false;
                
                var center = visionZone.transform.position;
                var extents = visionZone.Extents;

                var side = Random.Range(0, 4);
                Vector3 candidate;

                switch (side)
                {
                    case 0:
                        candidate = new Vector3(center.x - extents.x,
                            0f,
                            Random.Range(center.z - extents.y, center.z + extents.y));
                        break;
                    case 1:
                        candidate = new Vector3(center.x + extents.x,
                            0f,
                            Random.Range(center.z - extents.y, center.z + extents.y));
                        break;
                    case 2:
                        candidate = new Vector3(Random.Range(center.x - extents.x, center.x + extents.x),
                            0f,
                            center.z + extents.y);
                        break;
                    default:
                        candidate = new Vector3(Random.Range(center.x - extents.x, center.x + extents.x),
                            0f,
                            center.z - extents.y);
                        break;
                }

                var insideOther = visionZones.Any(p => p != visionZone && p.IsPointInsideVision(candidate));
                
                if (!insideOther)
                {
                    spawnPoint = candidate;
                    return true;
                }
            }

            return false;
        }
        
        private PlayerVision GetRandomVisionZone(List<PlayerVision> visionZones)
        {
            var visionZone = visionZones[Random.Range(0, visionZones.Count)];

            if (visionZone == null)
            {
                UpdatePlayerVisions();
                visionZone = visionZones.FirstOrDefault();
            }
            
            return visionZone;
        }
    }
}