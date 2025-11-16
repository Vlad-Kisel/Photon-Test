using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class SessionInfoView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _sessionNameText;
        [SerializeField] private TMP_InputField _playerId;

        private Game _game;
        private PlayerProvider _playerProvider;

        [Inject]
        public void Construct(Game game, PlayerProvider playerProvider)
        {
            _game = game;
            _playerProvider = playerProvider;
            
            
            _sessionNameText.text = _game.Runner.SessionInfo.Name;
            _sessionNameText.readOnly = true;

            _playerId.text = _game.Runner.UserId;
            
            _game.OnGameStart += UpdateView;
            _playerProvider.PlayersUpdated += UpdateView;
        }

        private void UpdateView()
        {
            _sessionNameText.text = _game.Runner.SessionInfo.Name;
            _playerId.text = _playerProvider.Players.FirstOrDefault(x => x.Value.HasInputAuthority).Key.ToString();
            
            _game.OnGameStart -= UpdateView;
        }
    }
}