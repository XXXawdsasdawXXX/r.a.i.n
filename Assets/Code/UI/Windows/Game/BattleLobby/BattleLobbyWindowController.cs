using Core.Localization;
using Core.ServiceLocator;
using CoreGame.Card.Logic;
using CoreGame.Card.Logic.Network;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.BattleLobby
{
    public class BattleLobbyWindowController : UIWindowController<BattleLobbyWindowView>
    {
        private NetworkBattleService _networkBattleService;
        private LocalizationService _localization;
        private BattleLobbyState _lastLobbyState;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _networkBattleService = Container.Instance.GetService<NetworkBattleService>();
            _localization = Container.Instance.GetService<LocalizationService>();
            view.Close();
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (view == null)
            {
                Debug.LogWarning("BattleLobbyWindowView is not assigned.");
                return;
            }

            if (flag)
            {
                _networkBattleService.LobbyStateChanged += _onLobbyStateChanged;
                view.ButtonCancel.Clicked += _onCancelClicked;
                view.ButtonStart.Clicked += _onStartClicked;
                if (_localization != null)
                {
                    _localization.LocaleChanged += _onLocaleChanged;
                }
            }
            else
            {
                _networkBattleService.LobbyStateChanged -= _onLobbyStateChanged;
                view.ButtonCancel.Clicked -= _onCancelClicked;
                view.ButtonStart.Clicked -= _onStartClicked;
                if (_localization != null)
                {
                    _localization.LocaleChanged -= _onLocaleChanged;
                }
            }
        }

        private void _onLobbyStateChanged(BattleLobbyState state)
        {
            if (view == null)
            {
                return;
            }

            if (!state.IsOpen || !state.ShouldShowLobby)
            {
                _lastLobbyState = default;
                Close();
                return;
            }

            _lastLobbyState = state;
            _applyLobbyTexts(state);

            view.ButtonStart.gameObject.SetActive(state.IsHost);
            view.ButtonStart.SetInteractable(state.CanStart);

            Open();
        }

        private void _onLocaleChanged()
        {
            if (!_lastLobbyState.IsOpen)
            {
                return;
            }

            _applyLobbyTexts(_lastLobbyState);
        }

        private void _applyLobbyTexts(BattleLobbyState state)
        {
            view.TextStatus.SetText(state.GetStatusText(_localization));
            view.TextHint.SetText(state.GetHintText(_localization));
        }

        private void _onCancelClicked()
        {
            _networkBattleService.RequestLeaveLobby();
            Close();
        }

        private void _onStartClicked()
        {
            _networkBattleService.RequestStartLobby();
        }
    }
}
