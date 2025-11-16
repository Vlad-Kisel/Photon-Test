using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerProvider : MonoBehaviour
    {
        [SerializeField, ReadOnly] private PlayerProviderState  _networkState;
        
        private Dictionary<int, Player> _cachedPlayers = new Dictionary<int, Player>();
        public IReadOnlyDictionary<int, Player> Players
        {
            get
            {
                if(_networkState.Runner.IsServer)
                    return _cachedPlayers;
                
                var notValidPlayer = _cachedPlayers
                    .Where(x => x.Value == null)
                    .Select(x => x.Key)
                    .ToList();
                
                foreach (var playerId in notValidPlayer)
                { 
                    _cachedPlayers.Remove(playerId);
                    Debug.LogError($"Player count mismatch {playerId}");
                }

                return _cachedPlayers;
            }
        }
        
        public event Action PlayersUpdated;
        public event Action<Player> PlayerAdded;
        public event Action<Player> PlayerRemoved;

        public void Init(PlayerProviderState networkState)
        {
            if (_networkState != null)
                _networkState.OnChanged -= OnNetworkPlayersChanged;

            _networkState = networkState;
            _cachedPlayers.Clear();

            _networkState.OnChanged += OnNetworkPlayersChanged;

            if(_networkState.IsValid())
                OnNetworkPlayersChanged();
        }
        
        public void Register(int id, Player player)
        {
            if (_networkState == null)
                return;
            
            Debug.Log($"Registering player {id}");
            _networkState.Players.Add(id, player);

            OnNetworkPlayersChanged();
        }

        public void Unregister(int id)
        {
            if (_networkState == null)
                return;
            
            Debug.Log($"Unregistering player {id}");
            _networkState.Players.Remove(id);

            OnNetworkPlayersChanged();
        }
        
        public Player GetNearPlayer(Vector3 position)
        {
            return Players
                .Select(x => x.Value)
                .OrderBy(p => (p.transform.position - position).sqrMagnitude)
                .FirstOrDefault();
        }
        
        private void OnNetworkPlayersChanged()
        {
            if (_networkState == null)
                return;
            
            var validPlayers = _networkState.Players
                .Where(x => IsValidPlayer(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (var player in validPlayers)
            {
                if (!_cachedPlayers.ContainsKey(player.Key))
                    PlayerAdded?.Invoke(player.Value);
            }

            foreach (var player in _cachedPlayers)
            {
                if (!validPlayers.ContainsKey(player.Key))
                    PlayerRemoved?.Invoke(player.Value);
            }

            _cachedPlayers = validPlayers;

            PlayersUpdated?.Invoke();
        }
        
        private bool IsValidPlayer(Player player)
        {
            return player != null && player.Object != null && player.Object.IsValid;
        }
    }
}