using CoreGame.Card.Data;

namespace UI.Windows.Game.Card
{
    public class CardWindowSelectionState
    {
        public CardBattleState PendingTargetCard;
        public BattleSide PendingMoveSide;
        public string PendingCardId;
        public string PendingMoveUnitId;
        public string PendingSummonCardId;
        public string PendingTargetCardActorId;
        public string PendingTargetCardActorSideHeroId;
        public bool IsMoveTargetSelection;
        public bool IsMoveCellSelection;
        public bool IsSummonCellSelection;
        public bool IsUnitTargetSelection;

        public void ClearMoveSelection()
        {
            PendingCardId = null;
            PendingMoveUnitId = null;
            PendingMoveSide = null;
            IsMoveTargetSelection = false;
            IsMoveCellSelection = false;
            PendingSummonCardId = null;
            IsSummonCellSelection = false;
        }

        public void ClearTargetSelection()
        {
            PendingTargetCard = null;
            PendingTargetCardActorId = null;
            PendingTargetCardActorSideHeroId = null;
            IsUnitTargetSelection = false;
        }

        public void ClearAllSelections()
        {
            ClearMoveSelection();
            ClearTargetSelection();
        }
    }
}
