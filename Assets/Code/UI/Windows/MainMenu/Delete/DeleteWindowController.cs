using System;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.MainMenu.Delete
{
    public class DeleteWindowController  : UIWindowController<DeleteWindowView>
    {
        private event Action _callback;
        

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonDelete.Clicked += _invokeCallback;
            }
            else
            {
                view.ButtonDelete.Clicked -= _invokeCallback;
            }
        }

        public void SetObserved(string objectName, Action success)
        {
            _callback = success;
            view.TextName.SetText($"Delete '{objectName}'?");
            view.TextName.gameObject.SetActive(!string.IsNullOrEmpty(objectName));
            view.ObjectIcon.SetActive(false);
        }

        public void SetObserved(Sprite objectSprite, Action success)
        {
            _callback = success;
            view.ImageIcon.SetSprite(objectSprite);
            view.ObjectIcon.SetActive(objectSprite != null);
        }

        public void Dispose()
        {
            _callback = null;
        }

        private void _invokeCallback()
        {
            _callback?.Invoke();
            _callback = null;
            
            view.TextName.SetText(string.Empty);
            
            Close();
        }
    }
}