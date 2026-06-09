using Core.Network;
using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.Game.Card.TurnEnergy
{
    public class CardStepEnergyController : UIWindowController<CardStepEnergyView>
    {
        private BattleService _battleService;
        private UserProvider _userProvider;
        private BattleModel _battleModel;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);
           
            if (flag)
            {
                _battleService.BattleStarted += _onBattleUpdated;
                _battleService.TurnStarted += _onBattleUpdated;
                _battleService.CardPlayed += _onBattleUpdated;
                _battleService.BattleFinished += _onBattleFinished;
            }
            else
            {
                _battleService.BattleStarted -= _onBattleUpdated;
                _battleService.TurnStarted -= _onBattleUpdated;
                _battleService.CardPlayed -= _onBattleUpdated;
                _battleService.BattleFinished -= _onBattleFinished;
            }
        }


        private void _onBattleUpdated(BattleModel model)
        {
            _battleModel = model;
            _refresh();
        }

        private void _onBattleFinished(BattleModel _)
        {
            _battleModel = null;
            view.Close();
        }

        private void _refresh()
        {
            if (_battleModel == null)
            {
                view.Close();
                return;
            }

            string myId = _getLocalHeroId();
            BattleSide mySide = _getMySide(_battleModel, myId);

            if (!_isMyTurn(_battleModel, myId))
            {
                view.Close();
                return;
            }

            view.Open();
            _updateEnergy(mySide.Hero);
        }

        private void _updateEnergy(BattleUnit unit)
        {
            view.SetValue(unit.Energy, unit.MaxEnergy);
        }

        private static BattleSide _getMySide(BattleModel battle, string playerId)
        {
            if (!string.IsNullOrEmpty(playerId))
            {
                if (battle.SideA.Hero.UnitId == playerId)
                {
                    return battle.SideA;
                }

                if (battle.SideB.Hero.UnitId == playerId)
                {
                    return battle.SideB;
                }
            }

            return battle.SideA;
        }

        private static bool _isMyTurn(BattleModel battle, string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            bool isSideA = battle.SideA.Hero.UnitId == playerId;

            return battle.Phase.Value switch
            {
                EBattlePhase.FirstSideTurn => isSideA,
                EBattlePhase.SecondSideTurn => !isSideA,
                _ => false
            };
        }

        private string _getLocalHeroId()
        {
            if (!string.IsNullOrEmpty(_userProvider.Id))
            {
                return _userProvider.Id;
            }

            Hero hero = _userProvider.GetHeroComponent<Hero>();
            return hero?.Model?.HeroId;
        }
    }
}