using Plugins.Demigiant.DOTween.Modules;
using UnityEngine;

namespace Code.CoreGame.Entities.Hero
{
    [CreateAssetMenu(fileName = "Settings_Hero", menuName = "Game/Settings/Hero")]
    public class HeroSettings : ScriptableObject
    {
        [field: Header("Colors")]
        [field: SerializeField] public ColorTweenData DamageTween { get; private set; }
    }
}