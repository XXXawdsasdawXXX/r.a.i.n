using Core.Save;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Windows.Base
{
    public abstract class UIWindowController<UIView> : Essential.Mono, IWindowController where UIView : UIWindowView
    {
        [SerializeField] protected UIView view;
        protected UIWindowManager windowManager;
        
        public virtual UniTask InitializeWindow(UIWindowManager manager)
        {
            windowManager = manager;

            return UniTask.CompletedTask;
        }

        public virtual void LoadWindow(GameModel model)
        {
        }

        public virtual void StartWindow()
        {
        }

        public virtual void SubscribeToEvents(bool flag)
        {
        }

        public void Open()
        {
            view.Open();
        }

        public void Close()
        {
            view.Close();
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (view == null && !TryGetComponent(out view))
            {
                view = gameObject.AddComponent<UIView>();
            }

            if (windowManager == null)
            {
                windowManager = GetComponentInParent<UIWindowManager>(true);
            }
        }
#endif
    }
}