using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class Enemy : NetworkBehaviour
    {
        [SerializeField] private Follow _follow;
        [SerializeField] private int _speed;
        
        [field: SerializeField] public HealthStat Health { get; private set; }
        [field: SerializeField] public Death Death { get; private set; }
        
        private PlayerProvider _playerProvider;
        
        [Inject]
        public void Construct(PlayerProvider playerProvider)
        {
            _playerProvider = playerProvider;
            
            _playerProvider.PlayersUpdated += TryFollow;
        }

        public override void FixedUpdateNetwork()
        {
            TryFollow();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if(HasStateAuthority)
                _playerProvider.PlayersUpdated -= TryFollow;
        }

        private void TryFollow()
        {
            if(!HasStateAuthority ||
               _playerProvider is null ||
               _follow.HasTarget) 
                return;
            
            var player = _playerProvider.GetNearPlayer(transform.position);
            
            if(player != null)
                Follow(player.transform);
        }

        private void Follow(Transform target)
        {
            _follow.Init(_speed);
            _follow.SetFollow(target);
        }
    }
}