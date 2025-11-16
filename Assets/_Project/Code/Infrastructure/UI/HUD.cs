using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private HealthView _healthView;
        [SerializeField] private LevelStatView _levelStatView;
        
        public void Init(Player player)
        {
            _healthView.Init(player.Stats.Health);
            _levelStatView.Init(player.Stats.Level);
        }
    }
}