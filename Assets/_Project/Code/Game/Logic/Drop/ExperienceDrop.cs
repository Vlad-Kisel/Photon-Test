using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class ExperienceDrop : ItemDrop
    {
        [Networked] private int _experience { get; set; }

        public override void Init(ItemDropStaticData staticData)
        {
            _experience = staticData.Amount;
        }

        protected override bool TryApplyInternal(GameObject target)
        {
            if (!target.TryGetComponent(out Player player))
                return false;
            
            player.Stats.Level.CollectExperience(_experience);
            return true;
        }
    }
}