using System;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.MainMenu.NewHero
{
    public class NewHeroWindowController : UIWindowController<NewHeroWindowView>
    {
        public event Action HeroCreated;
        
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
                LastGameExitTime = default
            });
            
            HeroCreated?.Invoke();
        }

        private void _onInputFieldChanged(string value)
        {
            view.ButtonCreate.SetInteractable(!string.IsNullOrEmpty(value));   
        }
    }
}