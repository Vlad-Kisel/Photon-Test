using System;
using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    [Serializable]
    public struct AttackStatState : INetworkStruct
    {
        [Networked] public int Damage {get; set;}
        [Networked] public float AttackSpeed{get; set;}
    }
}