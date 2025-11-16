using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class ProjectInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private Game _gamePrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ProjectInstaller>()
                .FromInstance(this)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<Game>()
                .FromComponentInNewPrefab(_gamePrefab)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<GameServicesWrapper>().AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<GameServicesResolver>()
                .AsSingle()
                .CopyIntoDirectSubContainers();
        }

        public void Initialize()
        {
            SceneManager.LoadScene(nameof(GameScene.Lobby));
        }
    }
}