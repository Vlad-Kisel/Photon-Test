using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Infrastructure.Network
{
    public class HealthView : MonoBehaviour
    {
        [SerializeField] private Slider _heathSlider;
        
        private IHealth _heath;

        public void Init(IHealth heath)
        {
            _heath = heath;
            
            heath.OnChanged += UpdateView;
            
            UpdateView();
        }
        
        private void OnDestroy()
        {
            if(_heath != null)
                _heath.OnChanged -= UpdateView;
        }

        private void UpdateView()
        {
            _heathSlider.maxValue = _heath.State.Max;
            _heathSlider.value = _heath.State.Current;
        }

    }
}