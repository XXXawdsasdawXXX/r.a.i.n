using Core.Localization;
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
        private LocalizationService _localization;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _gameStateMachine = Container.Instance.GetService<GameStateMachine>();
            _connectionHandler = Container.Instance.GetService<ConnectionHandler>();

            _localization = Container.Instance.GetService<LocalizationService>();
            _refreshIpLabel();

            view.InputFieldHostIP.SetTextWithoutNotify(_connectionHandler.LastJoinedIP);

            view.InputFieldHostIP.SetTextWithoutNotify(
                PlayerPrefs.GetString(ConnectionHandler.SAVE_KEY, view.InputFieldHostIP.Value));

            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonServer.Clicked += ButtonServerOnClicked;
                view.ButtonHost.Clicked += ButtonHostOnClicked;
                view.ButtonClient.Clicked += ButtonClientOnClicked;
                _localization.LocaleChanged += _onLocaleChanged;
            }
            else
            {
                view.ButtonServer.Clicked -= ButtonServerOnClicked;
                view.ButtonHost.Clicked -= ButtonHostOnClicked;
                view.ButtonClient.Clicked -= ButtonClientOnClicked;
                _localization.LocaleChanged -= _onLocaleChanged;
            }
        }

        private void _onLocaleChanged()
        {
            _refreshIpLabel();
        }

        private void _refreshIpLabel()
        {
            view.TextUserIP.SetText(_localization.Format(
                LocalizationTables.MainMenu,
                LocalizationKeys.MainMenu.ConnectionYourIp,
                ConnectionHandler.GetLocalIPAddress()));
        }

        private void ButtonServerOnClicked()
        {
            EnterGameAfterConnection(_connectionHandler.TryStartServerAsync()).Forget();
        }

        private void ButtonClientOnClicked()
        {
            PlayerPrefs.SetString(ConnectionHandler.SAVE_KEY, view.InputFieldHostIP.Value);

            EnterGameAfterConnection(_connectionHandler.TryConnectAsClientAsync(view.InputFieldHostIP.Value)).Forget();
        }

        private void ButtonHostOnClicked()
        {
            EnterGameAfterConnection(_connectionHandler.TryStartHostAsync()).Forget();
        }

        private async UniTaskVoid EnterGameAfterConnection(UniTask<bool> connectionTask)
        {
            if (!await connectionTask)
            {
                return;
            }

            _gameStateMachine.SwitchState(typeof(CoreGameState));
            view.Close();
        }
    }
}
