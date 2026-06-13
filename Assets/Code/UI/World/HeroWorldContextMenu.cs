using System;
using Core.GameLoop;
using Core.Localization;
using Core.ServiceLocator;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using FishNet;
using UI.Components;
using UnityEngine;

namespace UI.World
{
    public sealed class HeroWorldContextMenu : Essential.Mono, IInitializeListener, ISubscriber
    {
        public event Action Closed;

        public bool IsInitialized { get; set; }

        [SerializeField] private RectTransform _body;
        [SerializeField] private UIButton _duelButton;
        [SerializeField] private UIText _duelButtonLabel;

        private HeroContextTarget _target;
        private NetworkDuelService _duelService;
        private LocalizationService _localization;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        public UniTask Initialize()
        {
            _duelService = Container.Instance.GetService<NetworkDuelService>();
            _localization = Container.Instance.GetService<LocalizationService>();
            _setOpen(false);
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            if (_duelButton != null)
            {
                _duelButton.Clicked += _onDuelClicked;
            }
        }

        public void Unsubscribe()
        {
            if (_duelButton != null)
            {
                _duelButton.Clicked -= _onDuelClicked;
            }
        }

        public void Open(HeroContextTarget target)
        {
            _target = target;
            _refreshLabels();
            _setOpen(true);
        }

        public void Close()
        {
            if (!_isOpen)
            {
                return;
            }

            _setOpen(false);
            Closed?.Invoke();
        }

        private void _onDuelClicked()
        {
            if (_target == null || !InstanceFinder.IsClientStarted)
            {
                Close();
                return;
            }

            if (_getOnlinePlayerCount() < 2)
            {
                Debug.LogWarning("Duel requires at least two players online.");
                Close();
                return;
            }

            _duelService.OpenChallengeSetup(_target.HeroObjectId, _target.DisplayName);
            Close();
        }

        private void _refreshLabels()
        {
            _duelButton.gameObject.SetActive(_getOnlinePlayerCount() >= 2);
        }

        private void _setOpen(bool isOpen)
        {
            _isOpen = isOpen;
            if (_body != null)
            {
                _body.gameObject.SetActive(isOpen);
            }
        }

        private static int _getOnlinePlayerCount()
        {
            if (InstanceFinder.IsServerStarted)
            {
                return InstanceFinder.ServerManager.Clients.Count;
            }

            return InstanceFinder.IsClientStarted ? 1 : 0;
        }
    }
}
