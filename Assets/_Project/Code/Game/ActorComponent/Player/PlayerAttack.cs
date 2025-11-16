using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerAttack : NetworkBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float attackRadius = 6f;
        [SerializeField] private AttackStat _attackStat;

        [Header("Bullet")]
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;

        [Header("Target")]
        [SerializeField] private LayerMask enemyMask;

        [Networked] private float AttackProgress { get; set; }

        private HitBox _currentTarget;
        
        private bool HasTarget => _currentTarget != null && _currentTarget.IsAlive; 
        private int Damage => _attackStat.State.Damage;
        private float AttackSpeed => _attackStat.State.AttackSpeed;
        
        public override void Spawned()
        {
            if (HasStateAuthority)
                AttackProgress = 0f;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;

            UpdateTarget();

            if (!HasTarget)
                return;

            TryAttack();
        }

        private void UpdateTarget()
        {
            if (HasTarget)
                return;

            _currentTarget = FindTarget();
        }

        private HitBox FindTarget()
        {
            var hits = Physics.OverlapSphere(
                transform.position,
                attackRadius,
                enemyMask
            );

            var bestDistance = float.MaxValue;
            HitBox bestTarget = null;

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out HitBox hitBox))
                    continue;

                if (!hitBox.IsAlive)
                    continue;

                var distanceToTarget = Vector3.SqrMagnitude(
                    hit.transform.position - transform.position
                );

                if (distanceToTarget < bestDistance)
                {
                    bestDistance = distanceToTarget;
                    bestTarget = hitBox;
                }
            }

            return bestTarget;
        }

        private void TryAttack()
        {
            AttackProgress += Runner.DeltaTime * AttackSpeed;

            if (AttackProgress < 1f)
                return;

            while (AttackProgress >= 1f)
            {
                AttackProgress -= 1f;
                Fire();
            }
        }

        private void Fire()
        {
            if (!HasTarget)
                return;

            var targetPoint = _currentTarget.transform.position;

            var bullet = Runner.Spawn(
                bulletPrefab,
                bulletSpawnPoint.position,
                Quaternion.LookRotation(targetPoint - bulletSpawnPoint.position),
                Object.InputAuthority
            );

            bullet.Init(targetPoint);

            _currentTarget.Hit(Damage);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
