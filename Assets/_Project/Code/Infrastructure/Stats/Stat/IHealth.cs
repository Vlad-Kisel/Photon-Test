using System;
using _Project.Code.Infrastructure.Network;

public interface IHealth
{
    event Action OnChanged;
    
    HealthStatState State {get;}

    void Init(HealthStatState state);
    
    void TakeDamage(int amount);
}