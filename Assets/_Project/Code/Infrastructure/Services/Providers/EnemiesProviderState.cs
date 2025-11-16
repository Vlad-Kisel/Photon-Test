using System;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class EnemiesProviderState : NetworkBehaviour
    {
        [Networked, SerializeField, Capacity(Constants.MaxEnemyCount), OnChangedRender(nameof(OnNetworkEnemiesChanged))]
        public NetworkLinkedList<Enemy> Enemies => default;

        public event Action OnChanged;
        
        private void OnNetworkEnemiesChanged()
        {
            OnChanged?.Invoke();
        }
    }
}