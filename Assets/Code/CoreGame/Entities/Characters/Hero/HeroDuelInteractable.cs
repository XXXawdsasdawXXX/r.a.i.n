using Core.GameLoop;
using Core.ServiceLocator;
using CoreGame.Card.Logic;
using CoreGame.Common.Collisions;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using FishNet;
using UnityEngine;

namespace CoreGame.Entities.Characters.Hero
{
    public class HeroDuelInteractable : Essential.Mono, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }

        [SerializeField] private InteractionTrigger _interactionTrigger;
        [SerializeField] private Hero _hero;

        private NetworkDuelService _duelService;
        private bool _localPlayerInside;

        public UniTask Initialize()
        {
            _duelService = Container.Instance.GetService<NetworkDuelService>();
            if (_hero == null)
            {
                _hero = GetComponentInParent<Hero>();
            }

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            if (_hero == null || _hero.IsOwner || _interactionTrigger == null)
            {
                return;
            }

            _interactionTrigger.Enter += _onEnter;
            _interactionTrigger.Exit += _onExit;
            _interactionTrigger.InteractionPerformed += _onInteractionPerformed;
        }

        public void Unsubscribe()
        {
            if (_interactionTrigger == null)
            {
                return;
            }

            _interactionTrigger.Enter -= _onEnter;
            _interactionTrigger.Exit -= _onExit;
            _interactionTrigger.InteractionPerformed -= _onInteractionPerformed;
        }

        private void _onEnter(GameObject other)
        {
            if (_isLocalPlayer(other))
            {
                _localPlayerInside = true;
            }
        }

        private void _onExit(GameObject other)
        {
            if (_isLocalPlayer(other))
            {
                _localPlayerInside = false;
            }
        }

        private void _onInteractionPerformed()
        {
            if (!_localPlayerInside || _hero == null || _hero.IsOwner || !InstanceFinder.IsClientStarted)
            {
                return;
            }

            if (_getOnlinePlayerCount() < 2)
            {
                Debug.LogWarning("Duel requires at least two players online.");
                return;
            }

            string targetName = _hero.Name != null ? _hero.Name.Name : "Player";
            _duelService.OpenChallengeSetup(_hero.ObjectId.ToString(), targetName);
        }

        private static bool _isLocalPlayer(GameObject other)
        {
            Hero hero = other.GetComponentInParent<Hero>();
            return hero != null && hero.IsOwner;
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
