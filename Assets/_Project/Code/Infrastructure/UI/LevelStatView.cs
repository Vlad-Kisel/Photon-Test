using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Infrastructure.Network
{
    public class LevelStatView : MonoBehaviour
    {
        [SerializeField] private Slider _experienceSlider;
        
        private LevelStat _levelStat;

        public void Init(LevelStat levelStat)
        {
            _levelStat = levelStat;

            _levelStat.OnExperienceChanged += UpdateView;
            
            UpdateView();
        }

        private void UpdateView()
        {
            _experienceSlider.maxValue = _levelStat.GetExperienceForNextLevel();
            _experienceSlider.value = _levelStat.State.Experience;
        }

        private void OnDestroy()
        {
            if(_levelStat != null)
                _levelStat.OnChanged -= UpdateView;
        }
    }
}