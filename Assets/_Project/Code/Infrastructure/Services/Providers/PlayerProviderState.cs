using System;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerProviderState : NetworkBehaviour
    {
        [SerializeField , Networked, Capacity(Constants.MaxPlayerCount), OnChangedRender(nameof(OnNetworkPlayersChanged))]
        public NetworkDictionary<int, Player> Players => default;

        public event Action OnChanged;
        
        private void OnNetworkPlayersChanged()
        {
            OnChanged?.Invoke();
        }
    }
}