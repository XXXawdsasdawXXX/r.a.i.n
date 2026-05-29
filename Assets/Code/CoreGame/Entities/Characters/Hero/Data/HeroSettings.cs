using CoreGame.Card.Data;
using Plugins.Demigiant.DOTween.Modules;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    [CreateAssetMenu(fileName = "Settings_Hero", menuName = "Game/Settings/Hero")]
    public class HeroSettings : ScriptableObject
    {
        [field: Header("Params")]
        [field: SerializeField] public float MoveSpeed { get; private set; } = 100;
        
        [field: Space, Header("Colors")]
        [field: SerializeField] public ColorTweenData DamageTween { get; private set; }
    }
}