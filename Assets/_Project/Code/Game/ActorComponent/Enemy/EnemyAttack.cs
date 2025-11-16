using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class EnemyAttack : NetworkBehaviour
    {
        [SerializeField] private HealthStat _health;
        
        private Player _target;
        
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            TrySetTarget(hit.collider.gameObject);
        }
        
        private void FixedUpdate()
        {
            if(_target == null)
                return;
                
            Attack(_target);
            _target = null;
        }

        private void TrySetTarget(GameObject target)
        {
            if(!HasStateAuthority)
                return;
            
            if (target.TryGetComponent(out Player player))
                _target = player;
        }

        private void Attack(Player player)
        {
            player.Stats.Health.TakeDamage(Constants.EnemyAttack);
            _health.TakeDamage(_health.State.Max);
        }
    }
}