using System;
using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] private HealthStat _health;

    public event Action<Death> OnDie;
    
    public bool IsDead {get; private set;}
    
    private void Start()
    {
        _health.OnChanged += CheckDeath;
    }

    private void OnDestroy()
    {
        _health.OnChanged -= CheckDeath;
    }

    private void CheckDeath()
    {
        IsDead = _health.State.Current <= 0;
        
        if(!IsDead)
            return;
        
        _health.OnChanged -= CheckDeath;
        
        OnDie?.Invoke(this);
        OnDie = null;
    }
}