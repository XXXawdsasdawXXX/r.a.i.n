using Core.GameLoop;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreGame.Card.Logic.StateMachine
{
    public class DebugBattleModel : MonoBehaviour, IService, IInitializeListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
      
        [SerializeField] private BattleModel _battleModel;
        
        private BattleService _battleService;


        public UniTask Initialize()
        {
            _battleService = Container.Instance.GetService<BattleService>();

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _battleService.BattleStarted += _updateModel;
            _battleService.BattleFinished += _updateModel;
            _battleService.TurnStarted += _updateModel;
        }

        public void Unsubscribe()
        {
            _battleService.BattleStarted -= _updateModel;
            _battleService.BattleFinished -= _updateModel;
            _battleService.TurnStarted -= _updateModel;
        }

        private void _updateModel(BattleModel model)
        {
            _battleModel = model;
        }
    }
}