using System;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;
using UI.Windows.MainMenu.Delete;
using UnityEngine;

namespace UI.Windows.MainMenu.DeleteHero
{
    public class DeleteWindowController  : UIWindowController<DeleteWindowView>
    {
        public event Action PressDeleted;
        public bool IsInitialized { get; set; }
      
        private GameModel _gameModel;

        public override UniTask InitializeWindow()
        {
            _gameModel = Container.Instance.GetService<GameModel>();
            
            return UniTask.CompletedTask;
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonDelete.Clicked += _invokeDelete;
            }
            else
            {
                view.ButtonDelete.Clicked -= _invokeDelete;
            }
        }

        public void SetObservedObject(string objectName)
        {
            view.TextName.SetText(objectName);
            view.TextName.gameObject.SetActive(!string.IsNullOrEmpty(objectName));
        }
        
        public void SetObservedIcon(Sprite objectSprite)
        {
            view.ImageIcon.SetSprite(objectSprite);
            view.ObjectIcon.SetActive(objectSprite != null);
        }
        
        private void _invokeDelete()
        {
            PressDeleted?.Invoke();
        }
    }
}