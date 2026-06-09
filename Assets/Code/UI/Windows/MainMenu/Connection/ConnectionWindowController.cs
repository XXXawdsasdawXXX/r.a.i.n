using Core.Network;
using Core.ServiceLocator;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.MainMenu.Connection
{
    public class ConnectionWindowController : UIWindowController<ConnectionWindowView>
    {
        public bool IsInitialized { get; set; }

        private ConnectionHandler _connectionHandler;
        private GameStateMachine _gameStateMachine;
        private bool _isConnecting;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _connectionHandler = Container.Instance.GetService<ConnectionHandler>();
            _gameStateMachine = Container.Instance.GetService<GameStateMachine>();

            string localIp = ConnectionHandler.GetLocalIPAddress();
            string allIps = ConnectionHandler.GetAllLocalIPAddresses();
            string ipLabel = localIp == allIps ? localIp : $"{localIp} ({allIps})";
            view.TextUserIP.SetText($"your ip: {ipLabel}");

            view.InputFieldHostIP.SetTextWithoutNotify(
                PlayerPrefs.GetString(ConnectionHandler.SAVE_KEY, _connectionHandler.LastJoinedIP));

            return base.InitializeWindow(manager);
        }
        
        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonServer.Clicked += ButtonServerOnClicked;
                view.ButtonHost.Clicked += ButtonHostOnClicked;
                view.ButtonClient.Clicked += ButtonClientOnClicked;
            }
            else
            {
                view.ButtonServer.Clicked -= ButtonServerOnClicked;
                view.ButtonHost.Clicked -= ButtonHostOnClicked;
                view.ButtonClient.Clicked -= ButtonClientOnClicked;
            }
        }

        private void ButtonServerOnClicked()
        {
            _connectionHandler.StartServer();
            
            _gameStateMachine.SwitchState(typeof(CoreGameState));
            
            view.Close();
        }

        private void ButtonClientOnClicked()
        {
            _connectAsClientAsync().Forget();
        }

        private async UniTaskVoid _connectAsClientAsync()
        {
            if (_isConnecting)
            {
                return;
            }

            string hostIp = view.InputFieldHostIP.Value;
            PlayerPrefs.SetString(ConnectionHandler.SAVE_KEY, hostIp);

            _isConnecting = true;
            view.ButtonClient.SetInteractable(false);

            bool connected = await _connectionHandler.ConnectAsClientAsync(hostIp);
            _isConnecting = false;
            view.ButtonClient.SetInteractable(true);

            if (!connected)
            {
                return;
            }

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