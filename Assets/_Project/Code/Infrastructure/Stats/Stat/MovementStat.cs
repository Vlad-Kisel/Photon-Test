using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    public class MovementStat : BaseStat, IModifiableStat<MovementStatState>
    {
        [Networked] public ref MovementStatState NetworkState => ref MakeRef<MovementStatState>();

        public MovementStatState State => NetworkState;
        
        public void Init(MovementStatState state)
        {
            if (!HasStateAuthority)
                return;

            NetworkState = state;
        }
        
        public void Apply(in MovementStatState modifier, BinaryStatOperation operation)
        {
            NetworkState.Speed =  operation(State.Speed, modifier.Speed);
        }

        protected override void OnChangedInternal(NetworkBehaviourBuffer previousBuffer, NetworkBehaviourBuffer currentBuffer)
        {
        }
    }
}