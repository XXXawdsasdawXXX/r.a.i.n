using System;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;
using UI.Windows.MainMenu.Delete;

namespace UI.Windows.MainMenu.NewHero
{
    public class NewHeroWindowController : UIWindowController<NewHeroWindowView>
    {
        public bool IsInitialized { get; set; }
        public event Action HeroCreated;
        
        private GameModel _gameModel;
        private DeleteWindowController _deleteWindow;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _gameModel = Container.Instance.GetService<GameModel>();
            _deleteWindow = manager.GetWindow<DeleteWindowController>();

            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.InputFieldHeroName.Changed += _onInputFieldChanged;
                view.ButtonCreate.Clicked += _onButtonCreateClicked;
            }
            else
            {
                view.InputFieldHeroName.Changed -= _onInputFieldChanged;
                view.ButtonCreate.Clicked -= _onButtonCreateClicked;
            }
        }

        private void _onButtonCreateClicked()
        {
            _gameModel.Heroes.Add(new HeroModel
            {
                Name = view.InputFieldHeroName.Value,
                Health = 100,
                GameTime = default,
                ExitTime = default
            });
            
            HeroCreated?.Invoke();
        }

        private void _onInputFieldChanged(string value)
        {
            view.ButtonCreate.SetInteractable(!string.IsNullOrEmpty(value));   
        }
    }
}