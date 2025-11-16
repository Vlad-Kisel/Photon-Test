using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class HitBox : MonoBehaviour
    {
        [SerializeField] private HealthStat _health;
        [SerializeField] private Death _death;

        public bool IsAlive => !_death.IsDead;
        
        public void Hit(int damage)
        {
            if(IsAlive)
                _health.TakeDamage(damage);
        }
    }
}