using System;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class GameServicesResolver : IInitializable, IDisposable
    {
        private readonly GameServicesWrapper _gameServicesWrapper;
        private readonly GameLogic _gameLogic;

        public GameServicesResolver(
            GameServicesWrapper gameServicesWrapper,
            [Inject(Source = InjectSources.Local, Optional = true)]
            GameLogic gameLogic)
        {
            _gameServicesWrapper = gameServicesWrapper;
            
            _gameLogic = gameLogic;
        }

        public void Initialize()
        {
            _gameServicesWrapper.GameLogic = _gameLogic;
        }

        public void Dispose()
        {
            _gameServicesWrapper.Clear();
        }
    }
}