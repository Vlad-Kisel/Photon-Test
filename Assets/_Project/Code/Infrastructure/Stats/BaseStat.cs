using System;
using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    public abstract class BaseStat : NetworkBehaviour
    {
        public event Action OnChanged;
    
        protected ChangeDetector _changeDetector;

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
        }

        public override void Render()
        {
            foreach (var field in _changeDetector.DetectChanges( this, out var previousBuffer, out var currentBuffer))
            {
                OnChanged?.Invoke();
                
                OnChangedInternal(previousBuffer, currentBuffer);
                break;
            }
        }
        
        protected abstract void OnChangedInternal(NetworkBehaviourBuffer previousBuffer, NetworkBehaviourBuffer currentBuffer);
    }
}