using UnityEngine;

namespace UI.Windows.Base
{
    public abstract class UIWindowController<UIView> : Essential.Mono where UIView : UIWindowView
    {
        [SerializeField] protected UIView view;

        protected void OnEnable()
        {
            SubscribeToEvents(true);
        }

        private void OnDisable()
        {
            SubscribeToEvents(false);
        }

        protected virtual void SubscribeToEvents(bool flag)
        {
        }

        protected void OnValidate()
        {
            if (view == null && !TryGetComponent(out view))
            {
                view = gameObject.AddComponent<UIView>();
            }
        }
    }
}