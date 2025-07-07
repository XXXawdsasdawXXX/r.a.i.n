using System;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UI.Components;
using UI.Windows.Base;
using UI.Windows.MainMenu.Delete;
using UI.Windows.MainMenu.HeroSettings;
using UI.Windows.MainMenu.NewHero;

namespace UI.Windows.MainMenu.Hero
{
    public class HeroWindowController : UIWindowController<HeroWindowView>
    {
        public event Action HeroListChanged;

        private NewHeroWindowController _newHeroWindow;
        private DeleteWindowController _deleteWindow;

        private GameModel _gameModel;
        


        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _gameModel = Container.Instance.GetService<GameModel>();

            _newHeroWindow = manager.GetWindow<NewHeroWindowController>();
            _deleteWindow = manager.GetWindow<DeleteWindowController>();

            view.HeroesRadioGroup.Initialize();

            return base.InitializeWindow(manager);
        }

        public override void LoadWindow(GameModel model)
        {
            _updateHeroesList();

            _updateOptionButtonsView();

            _updateSelectedHeroView(model.LastHeroIndex.Value);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonNew.Clicked += _openNewHeroWindow;
                view.ButtonDelete.Clicked += _openDeleteHeroWindow;
                view.ButtonSettings.Clicked += _openHeroSettingsWindow;
                view.HeroesRadioGroup.Selected += _updateSelectedHeroView;
                view.HeroesRadioGroup.Selected += _updateSelectedHeroValue;

                _newHeroWindow.HeroCreated += _updateHeroesList;
            }
            else
            {
                view.ButtonNew.Clicked -= _openNewHeroWindow;
                view.ButtonDelete.Clicked -= _openDeleteHeroWindow;
                view.ButtonSettings.Clicked -= _openHeroSettingsWindow;
                view.HeroesRadioGroup.Selected -= _updateSelectedHeroView;
                view.HeroesRadioGroup.Selected -= _updateSelectedHeroValue;

                _newHeroWindow.HeroCreated -= _updateHeroesList;
            }
        }
        
        private void _openNewHeroWindow()
        {
            windowManager.OpenWindow<NewHeroWindowController>();
        }

        private void _openHeroSettingsWindow()
        {
            windowManager.OpenWindow<HeroSettingsWindowController>();
        }

        private void _openDeleteHeroWindow()
        {
            windowManager.OpenWindow<DeleteWindowController>();

            _deleteWindow.SetObserved(_gameModel.Hero.Name, () =>
            {
                _gameModel.Heroes.RemoveAt(_gameModel.LastHeroIndex.Value);

                int lastHeroIndex = _gameModel.Heroes.IndexOf(_gameModel.GetNearestHeroByExitTime());
                _gameModel.LastHeroIndex.Value = lastHeroIndex >= 0 ? lastHeroIndex : 0;
                
                _updateHeroesList();
                _updateOptionButtonsView();
            });
        }

        private void _updateHeroesList()
        {
            view.HeroesRadioGroup.Clear();

            if (_gameModel.Heroes != null && _gameModel.Heroes.Count > 0)
            {
                for (int i = 0; i < _gameModel.Heroes.Count; i++)
                {
                    HeroModel heroModel = _gameModel.Heroes[i];
                    UIText text = view.HeroesRadioGroup.Pool.GetNext();
                    text.SetIndex(i);
                    text.SetText(heroModel.Name);
                    text.Deselect();
                }

                view.HeroesRadioGroup.Pool.SortByIndex();
            }
            
            _updateSelectedHeroView(_gameModel.LastHeroIndex.Value);

            HeroListChanged?.Invoke();
        }

        private void _updateSelectedHeroValue(int heroIndex)
        {
            if (_gameModel.Heroes.Count > heroIndex && heroIndex >= 0)
            {
                _gameModel.LastHeroIndex.Value = heroIndex;
            }
        }
        
        private void _updateSelectedHeroView(int heroIndex)
        {
            if (_gameModel.Heroes.Count > heroIndex && heroIndex >= 0)
            {
                view.BodyHeroView.SetActive(true);
                view.HeroCard.SetModel(new UIHeroCardView.Model(
                    _gameModel.Heroes[heroIndex].Name,
                    _gameModel.Heroes[heroIndex].GameTime.ToString()));
                view.HeroesRadioGroup.Select(heroIndex);
            }
            else
            {
                view.BodyHeroView.SetActive(false);
            }
        }

        private void _updateOptionButtonsView()
        {
            bool isHasHero = _gameModel.Heroes.Count >= 0;
            view.ButtonDelete.SetInteractable(isHasHero);
            view.ButtonSettings.SetInteractable(isHasHero);
        }
    }
}