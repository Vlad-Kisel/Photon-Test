using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerLevelUpRewardService : IDisposable
    {
        private PlayerProvider _playerProvider;
        private ModifierStatFactory _modifierStatFactory;
        private readonly Game _game;

        private Dictionary<Player, Action> _levelUpHandlers = new();

        public PlayerLevelUpRewardService(Game game, ModifierStatFactory modifierStatFactory, PlayerProvider playerProvider)
        {
            _modifierStatFactory = modifierStatFactory;
            _playerProvider = playerProvider;

            _game = game;
            _game.OnGameStart += Init;
        }

        private void Init()
        {
            if(!_game.Runner.IsServer)
                return;

            foreach (var player in _playerProvider.Players)
                OnPlayerAdded(player.Value);

            _playerProvider.PlayerAdded -= OnPlayerAdded;
            _playerProvider.PlayerRemoved -= OnPlayerRemoved;
            
            _playerProvider.PlayerAdded += OnPlayerAdded;
            _playerProvider.PlayerRemoved += OnPlayerRemoved;
        }

        private void OnPlayerRemoved(Player player)
        {
            player.Stats.Level.OnLevelUp -= _levelUpHandlers[player];
            _levelUpHandlers.Remove(player);
        }

        private void OnPlayerAdded(Player player)
        {
            if(_levelUpHandlers.ContainsKey(player))
                return;
            
            Action handler = () => GiveRandomModifier(player);

            _levelUpHandlers[player] = handler;
            player.Stats.Level.OnLevelUp += handler;
        }

        private void GiveRandomModifier(Player player)
        {
            if(player.Stats.Level.State.Level == 1)
                return;
            
            var modifier = _modifierStatFactory.CreateRandom();
            player.Stats.ApplyModifier(modifier);

            player.Stats.Health.HealToMax();
        }

        public void Dispose()
        {
            _game.OnGameStart -= Init;
        }
    }
}