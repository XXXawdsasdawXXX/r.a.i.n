using Core.Localization;
using Core.ServiceLocator;
using CoreGame.Card.Logic;
using CoreGame.Card.Logic.Network;
using Cysharp.Threading.Tasks;
using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Duel
{
    public class DuelWindowController : UIWindowController<DuelWindowView>
    {
        private NetworkDuelService _duelService;
        private LocalizationService _localization;
        private DuelUiState _lastState;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _duelService = Container.Instance.GetService<NetworkDuelService>();
            _localization = Container.Instance.GetService<LocalizationService>();
            view.Close();
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (view == null)
            {
                return;
            }

            if (flag)
            {
                _duelService.DuelUiStateChanged += _onDuelUiStateChanged;
                view.ButtonPrimary.Clicked += _onPrimaryClicked;
                view.ButtonSecondary.Clicked += _onSecondaryClicked;
                if (_localization != null)
                {
                    _localization.LocaleChanged += _onLocaleChanged;
                }
            }
            else
            {
                _duelService.DuelUiStateChanged -= _onDuelUiStateChanged;
                view.ButtonPrimary.Clicked -= _onPrimaryClicked;
                view.ButtonSecondary.Clicked -= _onSecondaryClicked;
                if (_localization != null)
                {
                    _localization.LocaleChanged -= _onLocaleChanged;
                }
            }
        }

        private void _onDuelUiStateChanged(DuelUiState state)
        {
            if (view == null)
            {
                return;
            }

            if (!state.IsOpen)
            {
                _lastState = DuelUiState.Closed;
                Close();
                return;
            }

            _lastState = state;
            _applyTexts(state);
            Open();
        }

        private void _onLocaleChanged()
        {
            if (!_lastState.IsOpen)
            {
                return;
            }

            _applyTexts(_lastState);
        }

        private void _applyTexts(DuelUiState state)
        {
            int myGold = Mathf.Max(state.MyGold, _duelService.GetLocalGold());
            view.TextGold.SetText(_formatGold(myGold));

            switch (state.Role)
            {
                case EDuelUiRole.Setup:
                    view.TextTitle.SetText(_get(LocalizationKeys.CoreGame.DuelSetupTitle, "Challenge to duel"));
                    view.TextBody.SetText(_format(
                        LocalizationKeys.CoreGame.DuelSetupBody,
                        "Challenge {0} to a duel.",
                        state.OpponentName));
                    view.InputStake.gameObject.SetActive(true);
                    view.InputStake.SetTextWithoutNotify(Mathf.Max(1, state.GoldStake).ToString());
                    view.ButtonPrimary.gameObject.SetActive(true);
                    view.ButtonSecondary.gameObject.SetActive(true);
                    _setButtonText(view.ButtonPrimary, LocalizationKeys.CoreGame.DuelChallenge, "Challenge");
                    _setButtonText(view.ButtonSecondary, LocalizationKeys.CoreGame.DuelCancel, "Cancel");
                    break;
                case EDuelUiRole.Waiting:
                    view.TextTitle.SetText(_get(LocalizationKeys.CoreGame.DuelWaitingTitle, "Waiting for response"));
                    view.TextBody.SetText(_format(
                        LocalizationKeys.CoreGame.DuelWaitingBody,
                        "Waiting for {0} to accept the duel. Stake: {1} gold.",
                        state.OpponentName,
                        state.GoldStake));
                    view.InputStake.gameObject.SetActive(false);
                    view.ButtonPrimary.gameObject.SetActive(false);
                    view.ButtonSecondary.gameObject.SetActive(true);
                    _setButtonText(view.ButtonSecondary, LocalizationKeys.CoreGame.DuelCancel, "Cancel");
                    break;
                case EDuelUiRole.Invite:
                    view.TextTitle.SetText(_get(LocalizationKeys.CoreGame.DuelInviteTitle, "Duel invitation"));
                    view.TextBody.SetText(_format(
                        LocalizationKeys.CoreGame.DuelInviteBody,
                        "{0} challenges you to a duel. Stake: {1} gold.",
                        state.OpponentName,
                        state.GoldStake));
                    view.InputStake.gameObject.SetActive(false);
                    view.ButtonPrimary.gameObject.SetActive(true);
                    view.ButtonSecondary.gameObject.SetActive(true);
                    _setButtonText(view.ButtonPrimary, LocalizationKeys.CoreGame.DuelAccept, "Accept");
                    _setButtonText(view.ButtonSecondary, LocalizationKeys.CoreGame.DuelDecline, "Decline");
                    break;
            }
        }

        private void _onPrimaryClicked()
        {
            switch (_lastState.Role)
            {
                case EDuelUiRole.Setup:
                    if (int.TryParse(view.InputStake.Value, out int stake))
                    {
                        _duelService.SendChallenge(Mathf.Max(1, stake));
                    }
                    break;
                case EDuelUiRole.Invite:
                    _duelService.AcceptInvite();
                    Close();
                    break;
            }
        }

        private void _onSecondaryClicked()
        {
            switch (_lastState.Role)
            {
                case EDuelUiRole.Setup:
                case EDuelUiRole.Waiting:
                    _duelService.CancelDuel();
                    Close();
                    break;
                case EDuelUiRole.Invite:
                    _duelService.DeclineInvite();
                    Close();
                    break;
            }
        }

        private string _get(string key, string fallback)
        {
            return _localization != null
                ? _localization.Get(LocalizationTables.CoreGame, key, fallback)
                : fallback;
        }

        private string _format(string key, string fallback, params object[] args)
        {
            return _localization != null
                ? _localization.Format(LocalizationTables.CoreGame, key, args)
                : string.Format(fallback, args);
        }

        private string _formatGold(int gold)
        {
            return _format(LocalizationKeys.CoreGame.DuelGold, "Your gold: {0}", gold);
        }

        private void _setButtonText(UIButton button, string key, string fallback)
        {
            UIText label = button.GetComponentInChildren<UIText>(true);
            label?.SetText(_get(key, fallback));
        }
    }
}
