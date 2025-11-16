using System;
using Fusion;

[Serializable]
public struct HealthStatState : INetworkStruct
{
    [Networked] public int Current { get; set; }
    [Networked] public int Max { get; set; }
}