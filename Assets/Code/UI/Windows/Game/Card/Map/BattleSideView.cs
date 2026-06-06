using UI.Windows.Base;
using UnityEngine;
using CoreGame.Card.Data;

namespace UI.Windows.Game.Card.Map
{
    public class BattleSideView : UIWindowView
    {
        [SerializeField] private BattleGridCellView[] _frontCells = new BattleGridCellView[3];
        [SerializeField] private BattleGridCellView[] _backCells = new BattleGridCellView[3];

        public BattleGridCellView GetCell(CoreGame.Card.Data.EBattleLine line, int cellIndex)
        {
            BattleGridCellView[] cells = line == CoreGame.Card.Data.EBattleLine.Frontline
                ? _frontCells
                : _backCells;

            if (cells == null || cellIndex < 0 || cellIndex >= cells.Length)
            {
                return null;
            }

            return cells[cellIndex];
        }

        public void SetCellsHighlighted(bool value)
        {
            _setLineHighlighted(_frontCells, value);
            _setLineHighlighted(_backCells, value);
        }
        
        public void SetCellOccupied(EBattleLine line, int cellIndex, bool occupied)
        {
            BattleGridCellView cell = GetCell(line, cellIndex);
            if (cell != null)
            {
                cell.SetOccupied(occupied);
            }
        }
        
        public void SetCellHighlighted(EBattleLine line, int cellIndex, bool highlighted)
        {
            BattleGridCellView cell = GetCell(line, cellIndex);
            if (cell != null)
            {
                cell.SetHighlighted(highlighted);
            }
        }

        public void ClearOccupiedHighlights()
        {
            _setLineOccupied(_frontCells, false);
            _setLineOccupied(_backCells, false);
        }

        private static void _setLineHighlighted(BattleGridCellView[] cells, bool value)
        {
            if (cells == null)
            {
                return;
            }

            foreach (BattleGridCellView cell in cells)
            {
                if (cell == null)
                {
                    continue;
                }

                cell.SetHighlighted(value);
            }
        }
        
        private static void _setLineOccupied(BattleGridCellView[] cells, bool value)
        {
            if (cells == null)
            {
                return;
            }

            foreach (BattleGridCellView cell in cells)
            {
                if (cell == null)
                {
                    continue;
                }

                cell.SetOccupied(value);
            }
        }
    }
}