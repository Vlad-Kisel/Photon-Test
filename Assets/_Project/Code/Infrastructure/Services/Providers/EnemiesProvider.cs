using System;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class EnemiesProvider : MonoBehaviour
    {
        public event Action<Enemy> EnemyAdded;
        public event Action Changed;
        
        [SerializeField, ReadOnly] private EnemiesProviderState _networkState;
        [SerializeField] private List<Enemy> _cachedEnemies = new List<Enemy>();

        private bool _isInitialized;
        
        public IReadOnlyList<Enemy> Enemies
        {
            get
            {
                if (_networkState == null)
                    return new List<Enemy>();
                    
                if(_networkState.Runner.IsServer)
                    return _cachedEnemies;
                
                var count = _cachedEnemies.Count;
                _cachedEnemies.RemoveAll(x => x == null);
                
                if (count != _cachedEnemies.Count)
                    Debug.LogError("Enemies count mismatch");

                return _cachedEnemies;
            }
        }

        public void Init(EnemiesProviderState networkState)
        {
            if(_networkState != null)
                _networkState.OnChanged -= SyncEnemies;

            _networkState = networkState;
            _cachedEnemies.Clear();
            
            _networkState.OnChanged += SyncEnemies;
            
            if(_networkState.IsValid())
                SyncEnemies();
        }
        
        private void OnDestroy()
        {
            _networkState.OnChanged -= SyncEnemies;
        }
        
        public void Register(Enemy enemy)
        {
            _networkState.Enemies.Add(enemy);
            SyncEnemies();
        }

        public void Unregister(Enemy enemy)
        {
            _networkState.Enemies.Remove(enemy);
            SyncEnemies();
        }
        
        private void SyncEnemies()
        {
            if (!_networkState.Object.IsValid)
                return;

            var previous = new HashSet<Enemy>(_cachedEnemies);
            _cachedEnemies.Clear();

            foreach (var enemy in _networkState.Enemies)
            {
                if (enemy == null || enemy.IsDestroyed() || !enemy.IsValid())
                    continue;
                
                _cachedEnemies.Add(enemy);

                if (previous.Add(enemy))
                    EnemyAdded?.Invoke(enemy);
            }

            Changed?.Invoke();
        }
    }
}