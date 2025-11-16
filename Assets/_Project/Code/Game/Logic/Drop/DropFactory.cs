using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace _Project.Code.Infrastructure.Network
{
    public class DropFactory : MonoBehaviour
    {
        [SerializeField] private List<ItemDropStaticData> _items;

        private Game _game;

        [Inject]
        public void Construct(Game game)
        {
            _game = game;
        }

        public ItemDrop CreateRandomDrop(Vector3 position)
        {
            var itemData = _items[Random.Range(0, _items.Count)];

            var itemDrop = _game.Runner.Spawn(itemData.Prefab, position, Quaternion.identity, PlayerRef.MasterClient);
            itemDrop.transform.SetParent(transform);
            
            itemDrop.Init(itemData);
            
            return itemDrop;
        }
    }

    [Serializable]
    public class ItemDropStaticData
    {
        public DropType DropType;
        public ItemDrop Prefab;
        public int Amount;
    }
}