using Core.Network;
using Core.ServiceLocator;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public sealed class HeroContextTarget : NetworkBehaviour
    {
        [SerializeField] private Hero _hero;
        [SerializeField] private Collider2D _clickCollider;

        public Hero Hero => _hero != null ? _hero : GetComponentInParent<Hero>();
        public Collider2D ClickCollider => _clickCollider != null ? _clickCollider : GetComponent<Collider2D>();

        public string DisplayName =>
            Hero?.Name != null && !string.IsNullOrEmpty(Hero.Name.Name)
                ? Hero.Name.Name
                : "Player";

        public string HeroObjectId => Hero != null ? Hero.ObjectId.ToString() : string.Empty;

        
        public bool CanOpenContextMenu()
        {
            bool isLocal = _isLocalHero();
            bool isInBattle = _isInBattle();
            
            return IsClientInitialized
                   && Hero != null
                   && !isLocal
                   && !isInBattle;
        }

        public bool ContainsWorldPoint(Vector2 worldPoint)
        {
            Collider2D collider = ClickCollider;
            return collider != null && collider.OverlapPoint(worldPoint);
        }

        private bool _isLocalHero()
        {
            UserProvider userProvider = Container.Instance.GetService<UserProvider>();
            NetworkObject localHero = userProvider?.Hero;
            if (localHero == null || Hero == null)
            {
                return false;
            }
            
            return localHero == Hero.NetworkObject;
        }

        private bool _isInBattle()
        {
            return Hero?.Model != null && Hero.Model.InBattle;
        }
    }
}
