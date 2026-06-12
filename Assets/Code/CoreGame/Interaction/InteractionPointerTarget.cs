using CoreGame.InteractionObjects;
using UnityEngine;

namespace CoreGame.Interaction
{
    public sealed class InteractionPointerTarget : WorldPointerTarget
    {
        [SerializeField] private InteractionObject _interactionObject;
        [SerializeField] private bool _useRightClick = true;
        [SerializeField] private bool _useLeftClick;

        private void Awake()
        {
            if (_interactionObject == null)
            {
                _interactionObject = GetComponentInParent<InteractionObject>();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_useLeftClick)
            {
                LeftClicked += _onInteract;
            }

            if (_useRightClick)
            {
                RightClicked += _onInteract;
            }
        }

        protected override void OnDisable()
        {
            LeftClicked -= _onInteract;
            RightClicked -= _onInteract;

            base.OnDisable();
        }

        private void _onInteract(IWorldPointerTarget _)
        {
            _interactionObject?.StartInteraction();
        }
    }
}
