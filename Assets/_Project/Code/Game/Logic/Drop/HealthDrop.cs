using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class HealthDrop : ItemDrop
    {
        [Networked] private int _healAmount {get; set;}
        
        public override void Init(ItemDropStaticData staticData)
        {
            _healAmount = staticData.Amount;
        }

        protected override bool TryApplyInternal(GameObject target)
        {
            if(!target.TryGetComponent(out Player player))
                return false;
            
            player.Stats.Health.Heal(_healAmount);
            return true;
        }
    }
}