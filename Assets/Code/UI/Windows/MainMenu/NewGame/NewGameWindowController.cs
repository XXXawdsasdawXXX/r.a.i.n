using System;
using System.Collections.Generic;
using System.Linq;
using Core.Save;
using Core.ServiceLocator;
using Core.TIme;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.MainMenu.NewGame
{
    public class NewGameWindowController : UIWindowController<NewGameWindowView>
    {
        public bool IsInitialized { get; set; }

        public event Action AddedMadel; 
        
        private GameModel _gameModel;
        private UtcTime _utcTime;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _gameModel = Container.Instance.GetService<GameModel>();
            _utcTime = Container.Instance.GetService<UtcTime>();

            return base.InitializeWindow(manager);
        }

        public override void StartWindow()
        {
            _updateCreateButtonState(view.InputFieldGameName.Value);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonCreate.Clicked += _addWorld;
                view.InputFieldGameName.Changed += _updateCreateButtonState;
            }
            else
            {
                view.ButtonCreate.Clicked -= _addWorld;
                view.InputFieldGameName.Changed -= _updateCreateButtonState;
            }
        }

        private void _updateCreateButtonState(string value)
        {
            view.ButtonCreate.SetInteractable(!string.IsNullOrEmpty(value));
        }

        private void _addWorld()
        {
            bool containsName = _gameModel.Worlds.Any(world => world.Name.Equals(view.InputFieldGameName.Value));

            if (containsName)
            {
                return;
            }
            
            _gameModel.Worlds.Add(new WorldModel
            {
                Name = view.InputFieldGameName.Value,
                CreateTime = _utcTime.Current,
                ResourcesStorage = new Dictionary<int, int>(),
                SceneResources = new Dictionary<string, int>(),
            });

            _gameModel.LastWorldIndex.Value = _gameModel.Worlds.Count - 1;
            
            AddedMadel?.Invoke();
        }
    }
}