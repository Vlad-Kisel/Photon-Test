using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class AttackStat : BaseStat, IModifiableStat<AttackStatState>
    {
        [Networked, SerializeField] private ref AttackStatState NetworkState => ref MakeRef<AttackStatState>();

        public AttackStatState State => NetworkState;
        
        public void Init(AttackStatState state)
        {
            if (!HasStateAuthority)
                return;

            NetworkState = state;
        }

        public void Apply(in AttackStatState modifier, BinaryStatOperation operation)
        {
            NetworkState.AttackSpeed = operation(State.AttackSpeed, modifier.AttackSpeed);
            NetworkState.Damage = Mathf.FloorToInt( operation(State.Damage, modifier.Damage));
        }

        protected override void OnChangedInternal(NetworkBehaviourBuffer previousBuffer, NetworkBehaviourBuffer currentBuffer)
        {
        }
    }
}