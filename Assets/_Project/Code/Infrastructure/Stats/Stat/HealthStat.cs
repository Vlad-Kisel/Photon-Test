using System;
using _Project.Code.Infrastructure.Network;
using Fusion;
using UnityEngine;

public class HealthStat : NetworkBehaviour, IModifiableStat<HealthStatState>, IHealth
{
    [Networked, SerializeField] private ref HealthStatState NetworkState => ref MakeRef<HealthStatState>();
    
    private ChangeDetector _changeDetector;
    
    public event Action OnChanged;
    
    public HealthStatState State
    {
        get
        {
            if (Object == null)
                return default;
            
            return NetworkState;
        }
    }

    public void Init(HealthStatState state)
    {
        if (!HasStateAuthority)
            return;
        
        NetworkState = state;
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        OnChanged?.Invoke();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnChanged = null;
    }

    public override void Render()
    {
        foreach (var field in _changeDetector.DetectChanges(this))
        {
            OnChanged?.Invoke();
            break;
        }
    }
    
    public void HealToMax() =>
        Heal(NetworkState.Max);
    
    public void Heal(int amount) => 
        NetworkState.Current = Math.Clamp(NetworkState.Current + amount, NetworkState.Current, NetworkState.Max);

    public void TakeDamage(int amount)
    {
        if (!HasStateAuthority)
            return;
        
        NetworkState.Current = Mathf.Clamp(NetworkState.Current - amount, 0, NetworkState.Current);
        OnChanged?.Invoke();
    }

    public void Apply(in HealthStatState modifier, BinaryStatOperation operation)
    {
        NetworkState.Current = Mathf.FloorToInt(
            operation(NetworkState.Current, modifier.Current));

        NetworkState.Max = Mathf.FloorToInt(
            operation(NetworkState.Max, modifier.Max));
    }
}