using System;
using Core.Localization;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.MainMenu.Delete
{
    public class DeleteWindowController  : UIWindowController<DeleteWindowView>
    {
        private event Action _callback;
        private string _observedObjectName;
        

        public override void SubscribeToEvents(bool flag)
        {
            LocalizationService localization = LocalizationService.TryGet();

            if (flag)
            {
                view.ButtonDelete.Clicked += _invokeCallback;
                if (localization != null)
                {
                    localization.LocaleChanged += _onLocaleChanged;
                }
            }
            else
            {
                view.ButtonDelete.Clicked -= _invokeCallback;
                if (localization != null)
                {
                    localization.LocaleChanged -= _onLocaleChanged;
                }
            }
        }

        public void SetObserved(string objectName, Action success)
        {
            _callback = success;
            _observedObjectName = objectName;
            _refreshDeleteText();
            view.TextName.gameObject.SetActive(!string.IsNullOrEmpty(objectName));
            view.ObjectIcon.SetActive(false);
        }

        private void _onLocaleChanged()
        {
            if (string.IsNullOrEmpty(_observedObjectName))
            {
                return;
            }

            _refreshDeleteText();
        }

        private void _refreshDeleteText()
        {
            LocalizationService localization = LocalizationService.TryGet();
            string text = localization != null
                ? localization.Format(
                    LocalizationTables.MainMenu,
                    LocalizationKeys.MainMenu.DeleteConfirm,
                    _observedObjectName)
                : $"Delete '{_observedObjectName}'?";
            view.TextName.SetText(text);
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
            
            _observedObjectName = string.Empty;
            view.TextName.SetText(string.Empty);
            
            Close();
        }
    }
}