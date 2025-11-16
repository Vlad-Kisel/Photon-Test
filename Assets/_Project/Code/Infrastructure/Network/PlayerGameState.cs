using System;
using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerGameState : NetworkBehaviour
    {
        [Networked, OnChangedRender(nameof(OnHeapIdChanged))] public NetworkString<_16> HeapId {get; set;}
        
        public event Action OnChanged;

        public override void Spawned()
        {
            OnChanged?.Invoke();
        }

        private void OnHeapIdChanged()
        {
            OnChanged?.Invoke();
        }
    }
}