using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class LocalPlayerSetupService : MonoBehaviour
    {
        [SerializeField] private CameraFollow _cameraFollow;
        [SerializeField] private HUD _hud;
        
        private PlayerProvider _playerProvider;

        [Inject]
        public void Construct(PlayerProvider playerProvider)
        {
            _playerProvider = playerProvider;
        }

        public void Init()
        {
            TrySetupPlayer();
        }
        
        private void TrySetupPlayer()
        {
            _playerProvider.PlayersUpdated -= TrySetupPlayer;
            _playerProvider.PlayersUpdated += TrySetupPlayer;
            
            foreach (var player in _playerProvider.Players.Values)
            {
                if (!player.HasInputAuthority)
                    continue;

                _playerProvider.PlayersUpdated -= TrySetupPlayer;

                _hud.Init(player);
                _cameraFollow.Follow(player.transform);
                
                break;
            }
        }
    }
}