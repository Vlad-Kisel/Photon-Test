using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerPickUp : NetworkBehaviour
    {
        [SerializeField] private GameObject _pickupOwner;

        private void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent(out ItemDrop drop))
                PickUp(drop);
        }
        
        private void PickUp(ItemDrop item)
        {
            if(!HasStateAuthority)
                return;
            
            if(item.TryApply(_pickupOwner))
                item.Consume();
        }

        private void OnValidate()
        {
            if(_pickupOwner == null)
                _pickupOwner = gameObject;
        }
    }
}