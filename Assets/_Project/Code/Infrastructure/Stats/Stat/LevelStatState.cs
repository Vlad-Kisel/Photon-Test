using System;
using Fusion;

namespace _Project.Code.Infrastructure.Network
{
    [Serializable]
    public struct LevelStatState : INetworkStruct
    {
        public int Level;
        public int Experience;
    }
}