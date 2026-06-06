using UnityEngine;
using Essential;

namespace UI.Windows.Game.Card
{
    public enum EBattleHighlightColorType
    {
        AllyTarget = 0,
        EnemyTarget = 1,
        AllyCell = 2,
        EnemyCell = 3,
        OccupiedCell = 4
    }

    public static class BattleHighlightStyle
    {
        public static readonly Material HighlightMaterial = Resources.Load<Material>("Graphics/Materials/UI/material-ui-hightline");
        public static readonly Material OccupiedHighlightMaterial = HighlightMaterial;
        private static readonly int _outlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int _innerOutlineColorId = Shader.PropertyToID("_InnerOutlineColor");


        public static readonly Color AllyTargetColor = new Color(0.32f, 0.90f, 0.75f, 1f);
        public static readonly Color EnemyTargetColor = new Color(0.95f, 0.45f, 0.45f, 1f);
        
        public static readonly Color AllyCellColor = new Color(0.37f, 0.92f, 0.80f, 1f);
        public static readonly Color EnemyCellColor = new Color(0.98f, 0.55f, 0.55f, 1f);
        public static readonly Color OccupiedCellColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        public static Color GetColor(EBattleHighlightColorType colorType)
        {
            return colorType switch
            {
                EBattleHighlightColorType.AllyTarget => AllyTargetColor,
                EBattleHighlightColorType.EnemyTarget => EnemyTargetColor,
                EBattleHighlightColorType.AllyCell => AllyCellColor,
                EBattleHighlightColorType.EnemyCell => EnemyCellColor,
                EBattleHighlightColorType.OccupiedCell => OccupiedCellColor,
                _ => Color.clear
            };
        }

        public static Material ResolveHighlightMaterial(Material fallback)
        {
            if (IsHighlightCompatible(fallback))
            {
                return fallback;
            }

            return HighlightMaterial;
        }

        public static Material ResolveOccupiedHighlightMaterial(Material fallback)
        {
            if (IsHighlightCompatible(OccupiedHighlightMaterial))
            {
                return OccupiedHighlightMaterial;
            }

            return ResolveHighlightMaterial(fallback);
        }

        public static bool IsHighlightCompatible(Material material)
        {
            if (material == null)
            {
                return false;
            }

            return material.HasProperty(_outlineColorId) || material.HasProperty(_innerOutlineColorId);
        }
    }
}
