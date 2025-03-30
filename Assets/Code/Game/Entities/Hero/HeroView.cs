using Core.ServiceLocator;
using Game.Entities.Params;
using UnityEngine;

namespace Game.Entities.Hero
{
    public class HeroView : MonoView
    {
        [field: Header("Unity components")]
        [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
        
        [field: Space]
        
        [field: Header("Net components")]
        [field: SerializeField] public PersonName Name { get; private set; }
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public HeroMovement Movement { get; private set; }
        [field: SerializeField] public HeroColor Color { get; private set; }
        [field: SerializeField] public HeroAnimation Animation { get; private set; }
        [field: SerializeField] public HeroItemController ItemController { get; private set; }
    }
}