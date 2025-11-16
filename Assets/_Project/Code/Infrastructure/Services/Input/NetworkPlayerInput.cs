using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public struct NetworkPlayerInput : INetworkInput
    {
        public Vector2 Direction { get; set; }
    }
}