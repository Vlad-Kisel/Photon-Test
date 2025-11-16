using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class Stats : NetworkBehaviour
    {
        private List<IStatModifier> _modifiers = new List<IStatModifier>();
        
        private readonly Dictionary<Type, IStat> _stats = new Dictionary<Type, IStat>();
        private readonly Dictionary<Type, IStat> _modifiableStats = new Dictionary<Type, IStat>();
        
        [field: SerializeField] public HealthStat Health { get; private set; }
        [field: SerializeField] public AttackStat Attack { get; private set; }
        [field: SerializeField] public MovementStat Movement { get; private set; }
        [field: SerializeField] public LevelStat Level { get; private set; }
        
        private void Start()
        {
            RegisterStats();
        }

        public void ApplyModifier(IStatModifier modifier)
        {
            Debug.Log($"Applying modifier of {modifier == null}");
            modifier.Apply(this);
            _modifiers.Add(modifier);
        }
        
        public IModifiableStat<TState> GetModifiableStat<TState>()
        {
            return (IModifiableStat<TState>)_modifiableStats[typeof(TState)];
        }
        
        private void RegisterStats()
        {
            RegisterStat<HealthStatState, HealthStat>(Health);
            RegisterStat<AttackStatState, AttackStat>(Attack);
            RegisterStat<MovementStatState, MovementStat>(Movement);
            
            RegisterStat(Level);
        }

        private void RegisterStat<TStatState, TStat>(TStat stat)
            where TStat : IModifiableStat<TStatState>
        {
            RegisterStat(stat);
            
            _modifiableStats.Add(typeof(TStatState), stat);
        }
        
        private void RegisterStat<TStat>(TStat stat)
            where TStat : IStat
        {
            _stats.Add(typeof(TStat), stat);
        }
    }
}