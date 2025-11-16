using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class Player : NetworkBehaviour
    {
        [field: SerializeField]
        [Networked]
        public int PlayerId { get; private set; }
        
        [field: SerializeField]
        [Networked]
        public Stats Stats { get; private set; }

        public void Init(int id)
        {
            PlayerId = id;
        }
    }
}