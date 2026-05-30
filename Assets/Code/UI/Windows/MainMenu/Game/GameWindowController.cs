using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UI.Components;
using UI.Windows.Base;
using UI.Windows.MainMenu.Connection.Legacy;
using UI.Windows.MainMenu.Delete;
using UI.Windows.MainMenu.Hero;
using UI.Windows.MainMenu.NewGame;
using UnityEngine;

namespace UI.Windows.MainMenu.Game
{
    public class GameWindowController : UIWindowController<GameWindowView>
    {
        private GameModel _gameModel;
        private HeroWindowController _heroWindow;
        private ConnectionHandler _connectionHandler;
        private GameStateMachine _gameStateMachine;
        private DeleteWindowController _deleteWindow;
        private NewGameWindowController _newGameWindow;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _gameModel = Container.Instance.GetService<GameModel>();
            _connectionHandler = Container.Instance.GetService<ConnectionHandler>();
            _gameStateMachine = Container.Instance.GetService<GameStateMachine>();

            _heroWindow = manager.GetWindow<HeroWindowController>();
            _deleteWindow = manager.GetWindow<DeleteWindowController>();
            _newGameWindow = manager.GetWindow<NewGameWindowController>();
            
            view.WorldsRadioGroup.Initialize();
            
            return base.InitializeWindow(manager);
        }

        public override void LoadWindow(GameModel model)
        {
            view.TextUserIP.SetText($"IP: {ConnectionHandler.GetLocalIPAddress()}");
            
            _updateView();
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _heroWindow.HeroListChanged += _updateObjectLockerState;
                _newGameWindow.AddedMadel += _updateView;
                
                view.ButtonContinue.Clicked += _continueGame;
                view.ButtonDelete.Clicked += _openDeleteWindow;
                view.ButtonJoin.Clicked += _openJoinWindow;
                view.TextUserIP.Clicked += _copyIpToBuffer;
                view.WorldsRadioGroup.Selected += _changeSelectedWorld;
            }
            else
            {
                _heroWindow.HeroListChanged -= _updateObjectLockerState;
                _newGameWindow.AddedMadel -= _updateView;
            
                view.ButtonContinue.Clicked -= _continueGame;
                view.ButtonJoin.Clicked -= _openJoinWindow;
                view.TextUserIP.Clicked -= _copyIpToBuffer;
                view.WorldsRadioGroup.Selected -= _changeSelectedWorld;
            }
        }

        private void _openDeleteWindow()
        {
            _deleteWindow.SetObserved(
                _gameModel.World.Name, 
                success: () =>
                {
                    _gameModel.Worlds.RemoveAt(_gameModel.LastWorldIndex.Value);
                    
                    int lastWorldIndex = _gameModel.Worlds.IndexOf(_gameModel.GetNearestWorldByExitTime());
                    
                    _gameModel.LastWorldIndex.Value = lastWorldIndex >= 0 ? lastWorldIndex : 0;

                    _updateView();
                });
            
            windowManager.SwitchWindow<DeleteWindowController>();
        }

        private void _continueGame()
        {
            _connectionHandler.StartHost();
            
            _gameStateMachine.SwitchState(typeof(CoreGameState));
        }

        private void _updateObjectLockerState()
        {
            view.ObjectLocker.SetActive(_gameModel.Heroes.Count == 0);
        }

        private void _openJoinWindow()
        {
            windowManager.SwitchWindow<ConnectionWindowController>();
        }

        private void _changeSelectedWorld(int worldIndex)
        {
            _gameModel.LastWorldIndex.Value = worldIndex;
        }

        private void _copyIpToBuffer()
        {
            GUIUtility.systemCopyBuffer = $"{ConnectionHandler.GetLocalIPAddress()}";
        }

        private void _updateView()
        {
            _updateObjectLockerState();
            
            if (_gameModel.Worlds.Count > _gameModel.LastWorldIndex.Value)
            {
                view.WorldsRadioGroup.Clear();
                
                for (int i = 0; i < _gameModel.Worlds.Count; i++)
                {
                    WorldModel modelWorld = _gameModel.Worlds[i];
                    UIText worldTabView = view.WorldsRadioGroup.Pool.GetNext();

                    worldTabView.SetText(modelWorld.Name);
                    worldTabView.SetIndex(i);
                    worldTabView.Deselect();
                }

                view.WorldsRadioGroup.Select(_gameModel.LastWorldIndex.Value);
                
                view.WorldsRadioGroup.Pool.SortByIndex();
            }
            
            view.Scroll.SetViewPosition((float)_gameModel.LastWorldIndex.Value / _gameModel.Worlds.Count);
            
            view.ButtonContinue.SetInteractable(_gameModel.Worlds.Count > _gameModel.LastWorldIndex.Value);
            view.ButtonDelete.SetInteractable(_gameModel.Worlds.Count > _gameModel.LastWorldIndex.Value);
        }
    }
}