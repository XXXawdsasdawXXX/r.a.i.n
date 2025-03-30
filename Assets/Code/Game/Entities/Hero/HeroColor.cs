using FishNet.Object;
using UnityEngine;

namespace Game.Entities.Hero
{
    public class HeroColor : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public override void OnStartClient()
        {
            base.OnStartClient();

            enabled = IsOwner;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeColorServer(Color.red);
            }
        }

        [ObserversRpc]
        private void ChangeColorObserver(Color color)
        {
            _spriteRenderer.color = color;
        }

        [ServerRpc]
        private void ChangeColorServer(Color color)
        {
            ChangeColorObserver(color);
        }
    }
}