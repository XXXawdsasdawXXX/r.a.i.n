using Code.CoreGame.Installers;
using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Connection
{
    public class ConnectionWindowController : UIWindowController<ConnectionWindowView>, IInitializeListener
    {
        public bool IsInitialized { get; set; }
        
        [SerializeField] private ConnectionHandler _connectionHandler;
        
        private GameStateMachine _gameStateMachine;

        public UniTask Initialize()
        {
            _gameStateMachine = Container.Instance.GetService<GameStateMachine>();
            
            view.TextUserIP.SetText("your ip: " + ConnectionHandler.GetLocalIPAddress());
          
            view.InputFieldHostIP.SetTextWithoutNotify(_connectionHandler.LastJoinedIP);
            
            view.Open();
            
            return UniTask.CompletedTask;
        }
        
        protected override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonHost.Clicked += ButtonHostOnClicked;
                view.ButtonClient.Clicked += ButtonClientOnClicked;
            }
            else
            {
                view.ButtonHost.Clicked -= ButtonHostOnClicked;
                view.ButtonClient.Clicked -= ButtonClientOnClicked;
            }
        }

        private void ButtonClientOnClicked()
        {
            _connectionHandler.ConnectAsClient(view.InputFieldHostIP.Value);
            
            _gameStateMachine.SwitchState(typeof(CoreGameState));
            
            view.Close();
        }

        private void ButtonHostOnClicked()
        {
            _connectionHandler.StartHost();
            
            _gameStateMachine.SwitchState(typeof(CoreGameState));
            
            view.Close();
        }
    }
}