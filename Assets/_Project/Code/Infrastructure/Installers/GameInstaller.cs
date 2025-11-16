using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private PlayerProvider _playerProvider;
        [SerializeField] private PlayerFactory _playerFactory;

        [SerializeField] private EnemiesProvider _enemiesProvider;
        [SerializeField] private EnemyFactory _enemyFactory;
        
        [SerializeField] private DropFactory _dropFactory;
        [SerializeField] private ModifierStatFactory _modifierStatFactory;

        [SerializeField] private GameLogic _gameLogic;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameInstaller>().FromInstance(this).AsSingle().NonLazy();
            
            Container.Bind<PlayerProvider>()
                .FromInstance(_playerProvider)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<PlayerFactory>()
                .FromInstance(_playerFactory)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<EnemiesProvider>()
                .FromInstance(_enemiesProvider)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<EnemyFactory>()
                .FromInstance(_enemyFactory)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<DropFactory>()
                .FromInstance(_dropFactory)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<ModifierStatFactory>()
                .FromInstance(_modifierStatFactory)
                .AsSingle()
                .NonLazy();

            Container.Bind<GameLogic>()
                .FromInstance(_gameLogic)
                .AsSingle()
                .NonLazy();

            Container.Bind<InputService>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<PlayerLevelUpRewardService>().AsSingle().NonLazy();
        }
    }
}