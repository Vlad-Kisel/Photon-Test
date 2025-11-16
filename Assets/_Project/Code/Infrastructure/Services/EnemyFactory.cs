using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class EnemyFactory : MonoBehaviour
    {
        [SerializeField] private Enemy _enemyPrefab;
        
        private DiContainer _container;
        public EnemiesProvider _enemiesProvider;
        private Game _game;

        [Inject]
        public void Construct(EnemiesProvider enemiesProvider, Game game, DiContainer container)
        {
            _game = game;
            _enemiesProvider = enemiesProvider;
            _container = container;

            _game.OnGameStart += OnGameStart;
        }
        
        public Enemy Create(Vector3 position, Quaternion rotation)
        {
            var enemy = _game.Runner.Spawn(_enemyPrefab, position, rotation);

            InitializeEnemy(enemy);
            
            _enemiesProvider.Register(enemy);
            
            return enemy;
        }
        
        private void OnGameStart() 
        {
            if(_game.Runner.IsServer)
                return;
            
            InitializeExistingEnemies();
            
            _enemiesProvider.EnemyAdded += InitializeEnemy;
        }
        
        private void OnDestroy()
        {
            _enemiesProvider.EnemyAdded -= InitializeEnemy;
        }

        private void InitializeExistingEnemies()
        {
            foreach (var enemy in _enemiesProvider.Enemies)
                InitializeEnemy(enemy);
        }

        private void RegisterEnemy(Enemy enemy)
        {
            enemy.Death.OnDie += (death) =>
            {
                var enemy = death.gameObject.GetComponent<Enemy>();
                _enemiesProvider.Unregister(enemy);
                _game.Runner.Despawn(enemy.Object);
            };
        }

        private void InitializeEnemy(Enemy enemy)
        {
            _container.InjectGameObject(enemy.gameObject);
            
            if(_game.Runner.IsServer)
                RegisterEnemy(enemy);
            
            InitializeHealthState(enemy);
            BindHealthView(enemy);
        }

        private void InitializeHealthState(Enemy enemy)
        {
            if (!_game.Runner.IsServer)
                return;

            enemy.Health.Init(new HealthStatState
            {
                Current = Constants.StartEnemyHealth,
                Max = Constants.MaxEnemyHealth
            });
        }
        
        private void BindHealthView(Enemy enemy)
        {
            var healthView = enemy.GetComponentInChildren<HealthView>();
            
            if (healthView == null)
                return;

            healthView.Init(enemy.Health);
        }
    }
}