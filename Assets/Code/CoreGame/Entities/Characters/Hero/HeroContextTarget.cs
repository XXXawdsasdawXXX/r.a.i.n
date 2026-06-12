using Core.Network;
using Core.ServiceLocator;
using FishNet.Object;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public sealed class HeroContextTarget : NetworkBehaviour
    {
        private const float PickRadiusFallback = 0.85f;

        [SerializeField] private Hero _hero;
        [SerializeField] private Collider2D _clickCollider;

        public Hero Hero => _hero != null ? _hero : NetworkObject.GetComponent<Hero>();
        public Collider2D ClickCollider => _clickCollider != null ? _clickCollider : GetComponent<Collider2D>();

        public string DisplayName =>
            Hero?.Name != null && !string.IsNullOrEmpty(Hero.Name.Name)
                ? Hero.Name.Name
                : "Player";

        public string HeroObjectId => Hero != null ? Hero.ObjectId.ToString() : string.Empty;

        public bool CanOpenContextMenu()
        {
            if (Hero == null || !Hero.IsClientInitialized)
            {
                return false;
            }

            if (_isLocalHero())
            {
                return false;
            }

            return !_isInBattle();
        }

        public bool OverlapsPointer(Vector2 worldPoint)
        {
            if (Hero == null)
            {
                return false;
            }

            Collider2D[] colliders = Hero.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D collider in colliders)
            {
                if (collider == null || !collider.enabled)
                {
                    continue;
                }

                if (_colliderContainsPoint(collider, worldPoint))
                {
                    return true;
                }
            }

            return Vector2.Distance(worldPoint, Hero.transform.position) <= PickRadiusFallback;
        }

        private static bool _colliderContainsPoint(Collider2D collider, Vector2 worldPoint)
        {
            if (collider.OverlapPoint(worldPoint))
            {
                return true;
            }

            Bounds bounds = collider.bounds;
            return worldPoint.x >= bounds.min.x
                   && worldPoint.x <= bounds.max.x
                   && worldPoint.y >= bounds.min.y
                   && worldPoint.y <= bounds.max.y;
        }

        private bool _isLocalHero()
        {
            if (Hero.IsOwner)
            {
                return true;
            }

            UserProvider userProvider = Container.Instance.GetService<UserProvider>();
            if (userProvider?.Hero == null)
            {
                return false;
            }

            return userProvider.Hero.ObjectId == Hero.ObjectId;
        }

        private bool _isInBattle()
        {
            return Hero.Model != null && Hero.Model.InBattle;
        }
    }
}
