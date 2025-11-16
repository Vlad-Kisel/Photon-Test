using System;
using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class LevelStat : BaseStat , IStat
    {
        [Networked, SerializeField] private ref LevelStatState NetworkState => ref MakeRef<LevelStatState>();

        public event Action OnLevelUp;
        public event Action OnExperienceChanged;
        
        public LevelStatState State => NetworkState;
        
        public void Init(LevelStatState state)
        {
            if (!HasStateAuthority)
                return;

            NetworkState = state;
        }
        
        public void CollectExperience(int experience)
        {
            NetworkState.Experience += Mathf.Max(0, experience);

            while (NetworkState.Experience >= GetExperienceForNextLevel())
            {
                NetworkState.Experience -= GetExperienceForNextLevel();
                NetworkState.Level++;
            }
        }
        
        public int GetExperienceForNextLevel()
        {
            return Mathf.FloorToInt(100 * Mathf.Pow(NetworkState.Level, Constants.LevelExperienceGrowthFactor));
        }
        
        protected override void OnChangedInternal(NetworkBehaviourBuffer previousBuffer, NetworkBehaviourBuffer currentBuffer)
        {
            var reader = GetPropertyReader<LevelStatState>(nameof(NetworkState));
            (var previous, var current) =  reader.Read(previousBuffer, currentBuffer);

            if (previous.Experience != current.Experience)
            {
                Debug.LogWarning(current.Experience);
                OnExperienceChanged?.Invoke();
            }

            if(previous.Level != current.Level)
                OnLevelUp?.Invoke();
        }
    }
}