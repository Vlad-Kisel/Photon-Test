using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public abstract class ItemDrop : NetworkBehaviour
    {
        public bool IsConsume;

        public abstract void Init(ItemDropStaticData staticData);
        
        public bool TryApply(GameObject target)
        {
            if(IsConsume)
                return false;
            
            return TryApplyInternal(target);
        }

        public void Consume()
        {
            if(IsConsume)
                return;
            
            IsConsume = true;
            Runner.Despawn(Object);
        }
        
        protected abstract bool TryApplyInternal(GameObject target);
    }
}