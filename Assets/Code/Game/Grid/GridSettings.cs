using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Grid
{
    [CreateAssetMenu(fileName = "Settings_Grid", menuName = "Game/Settings/Grid")]
    public class GridSettings : ScriptableObject
    {
        [Serializable]
        private struct TileData
        {
            public ETileType TileType;
            public TileBase[] Tiles;
        }
        
        [SerializeField] private TileData[] _tiles;
        
        private readonly Dictionary<TileBase, ETileType> _tileTypes = new();
        
        public ETileType GetType(TileBase tileBase)
        {
            if (tileBase == null)
            {
                return ETileType.None;
            }
            
            if (_tileTypes.ContainsKey(tileBase))
            {
                return _tileTypes[tileBase];
            }

            foreach (TileData data in _tiles)
            {
                if (data.Tiles.Contains(tileBase))
                {
                    _tileTypes.Add(tileBase, data.TileType);

                    return data.TileType;
                }
            }

            return ETileType.None;
        }
    }
}