using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace _Project.Code.Infrastructure.Network
{
    public class ModifierStatFactory : MonoBehaviour
    {
        [SerializeField] private List<ModifierStatStaticData> _staticDatas;
        
        private Game _game;

        [Inject]
        public void Construct(Game game)
        {
            _game = game;
        }
        
        public IStatModifier CreateRandom()
        {
            if(!_game.Runner.IsServer)
                return new MockModifier();
            
            var index = Random.Range(0, _staticDatas.Count);
            
            var staticData = _staticDatas[index];
            
            return GetModifiedStat(staticData);
        }

        private IStatModifier GetModifiedStat(ModifierStatStaticData staticData)
        {
            if (staticData.Health.IsNotDefault())
                return new StatModifier<HealthStat, HealthStatState>(staticData.Health, staticData.OperationType, new DefaultModifierApplier());
            if(staticData.Attack.IsNotDefault())
                return new StatModifier<AttackStat, AttackStatState>(staticData.Attack, staticData.OperationType, new DefaultModifierApplier());
            if(staticData.Movement.IsNotDefault())
                return new StatModifier<MovementStat, MovementStatState>(staticData.Movement, staticData.OperationType, new DefaultModifierApplier());
            
            return null;
        }
        
        [Serializable]
        public class ModifierStatStaticData
        {
            [field: SerializeField] public HealthStatState Health {get; private set;}
            [field: SerializeField] public MovementStatState Movement {get; private set;}
            [field: SerializeField] public  AttackStatState Attack {get; private set;}
            
            [field: SerializeField] public ModifierOperationType OperationType { get; private set;}
        }
        
        private class MockModifier : IStatModifier
        {
            public void Apply(Stats stats) { }
        }
    }
}