using System;
using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    [Serializable]
    public struct MovementStatState : INetworkStruct
    {
        public float Speed;
    }
}