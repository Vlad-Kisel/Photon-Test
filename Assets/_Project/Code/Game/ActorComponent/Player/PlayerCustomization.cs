using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerCustomization : MonoBehaviour
    {
        [SerializeField] private Transform hatRoot;
        [SerializeField] private List<HatCustomization> _hatsCatalog;
        [SerializeField] private PlayerGameState _playerGameState;
        
        private Dictionary<string, HatCustomization> _hatsById;
        
        private GameObject _currentHatInstance;
        
        private void Start()
        {
            _hatsById = new Dictionary<string, HatCustomization>();

            foreach (var hat in _hatsCatalog)
                _hatsById.Add(hat.HatId, hat);
            
            _playerGameState.OnChanged += UpdateHat;
            
            UpdateHat();
        }

        private void OnDestroy()
        {
            _playerGameState.OnChanged -= UpdateHat;
        }

        private void UpdateHat()
        {
            ApplyHat(_playerGameState.HeapId.Value);
        }

        private void ApplyHat(string hatId)
        {
            if (_currentHatInstance != null)
                Destroy(_currentHatInstance);

            _currentHatInstance = Instantiate(
                _hatsById[hatId].Prefab,
                hatRoot.position,
                hatRoot.rotation,
                hatRoot
            );
        }

        [Serializable]
        public class HatCustomization
        {
            public string HatId;
            public GameObject Prefab; 
        }
    }
}