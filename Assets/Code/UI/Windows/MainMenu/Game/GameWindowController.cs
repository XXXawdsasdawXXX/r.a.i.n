using Core.GameLoop;
using Core.Network;
using Core.Save;
using Core.ServiceLocator;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UI.Components;
using UI.Windows.Base;
using UI.Windows.MainMenu.Connection.Legacy;
using UI.Windows.MainMenu.Hero;
using UnityEngine;

namespace UI.Windows.MainMenu.Game
{
    public class GameWindowController : UIWindowController<GameWindowView>
    {
        public bool IsInitialized { get; set; }
        
        private GameModel _gameModel;
        private HeroWindowController _heroWindow;
        private ConnectionHandler _connectionHandler;
        private GameStateMachine _gameStateMachine;

        public override UniTask InitializeWindow()
        {
            _gameModel = Container.Instance.GetService<GameModel>();
            _connectionHandler = Container.Instance.GetService<ConnectionHandler>();
            _gameStateMachine = Container.Instance.GetService<GameStateMachine>();
            
            view.WorldsRadioGroup.Initialize();
            
            return UniTask.CompletedTask;
        }

        public override void LoadWindow(GameModel model)
        {
            view.TextUserIP.SetText($"IP: {ConnectionHandler.GetLocalIPAddress()}");
            
            _updateObjectLockerState();
            
            _updateWorldList();
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                windowManager.GetWindow<HeroWindowController>().HeroListChanged += _updateObjectLockerState;
            
                view.ButtonContinue.Clicked += _continueGame;
                view.ButtonJoin.Clicked += _openJoinWindow;
                view.TextUserIP.Clicked += _copyIpToBuffer;
                view.WorldsRadioGroup.Selected += _changeSelectedWorld;
            }
            else
            {
                windowManager.GetWindow<HeroWindowController>().HeroListChanged -= _updateObjectLockerState;
            
                view.ButtonContinue.Clicked -= _continueGame;
                view.ButtonJoin.Clicked -= _openJoinWindow;
                view.TextUserIP.Clicked -= _copyIpToBuffer;
                view.WorldsRadioGroup.Selected -= _changeSelectedWorld;
            }
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
            windowManager.OpenWindow<ConnectionWindowController>();
        }

        private void _changeSelectedWorld(int worldIndex)
        {
            _gameModel.LastWorldIndex.Value = worldIndex;
        }

        private void _copyIpToBuffer()
        {
            GUIUtility.systemCopyBuffer = $"{ConnectionHandler.GetLocalIPAddress()}";
        }

        private void _updateWorldList()
        {
            if (_gameModel.Worlds.Count > _gameModel.LastWorldIndex.Value)
            {
                view.WorldsRadioGroup.Pool.DisableAll();
            
                foreach (WorldModel modelWorld in _gameModel.Worlds)
                {
                    UIText worldTabView = view.WorldsRadioGroup.Pool.GetNext();
                
                    worldTabView.SetText(modelWorld.Name);
                }
                
                view.WorldsRadioGroup.Pool.Enabled[_gameModel.LastWorldIndex.Value].Select();
            }
            
            view.ButtonContinue.SetInteractable(_gameModel.Worlds.Count > _gameModel.LastWorldIndex.Value);
            view.ButtonDelete.SetInteractable(_gameModel.Worlds.Count > _gameModel.LastWorldIndex.Value);
        }
    }
}