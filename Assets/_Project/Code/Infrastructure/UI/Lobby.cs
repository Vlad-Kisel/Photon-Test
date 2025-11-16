using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Code.Infrastructure.Network
{
    public class Lobby : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _lobbyNameInputField;
        
        [SerializeField] private Button _joinLobbyByNameButton;
        [SerializeField] private Button _joinLobbyAnyButton;
        
        private Game _game;

        [Inject]
        public void Construct(Game game)
        {
            _game = game;
        }
        
        public void Start()
        {
            JoinLobby(null);
            
            _joinLobbyAnyButton.onClick.AddListener(() => JoinLobby(null));
            _joinLobbyByNameButton.onClick.AddListener(() => JoinLobby(_lobbyNameInputField.text));
        }

        private void OnDestroy()
        {
            _joinLobbyAnyButton.onClick.RemoveAllListeners();
            _joinLobbyByNameButton.onClick.RemoveAllListeners();
        }

        private void JoinLobby(string sessionName) =>
            _game.StartGame(sessionName);
    }
}