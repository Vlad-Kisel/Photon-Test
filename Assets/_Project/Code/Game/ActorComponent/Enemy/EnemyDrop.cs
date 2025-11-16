using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class EnemyDrop : NetworkBehaviour
    {
        [SerializeField] private Death _death;
        
        private DropFactory _factory;

        [Inject]
        public void Construct(DropFactory factory)
        {
            _factory = factory;
        }

        public override void Spawned()
        {   
            _death.OnDie += SpawnDrop;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _death.OnDie -= SpawnDrop;
        }

        private void SpawnDrop(Death death)
        {
            if(HasStateAuthority)
                _factory.CreateRandomDrop(death.transform.position);
            
            _death.OnDie -= SpawnDrop;
        }
    }
}