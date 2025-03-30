using Core.Extensions;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Tilemaps;

namespace Game.Grid
{
    [Preserve]
    public class GridService : IService, IInitializeListener
    {
        private GridView _view;
        private GridSettings _settings;

        public UniTask GameInitialize()
        {
            _view = Container.Instance.GetView<GridView>();
            _settings = Container.Instance.GetConfig<GridSettings>();

            return UniTask.CompletedTask;
        }

        public float2 GetTilePosition(Vector3 worldPosition)
        {
            Vector3Int cellPos = WorldToCell(worldPosition);
            
            return cellPos.AsFloat2();
        }
        
        public ETileType GetTileType(Vector3 worldPosition)
        {
            Vector3Int cell = WorldToCell(worldPosition);
            TileBase tileBase = GetTile(cell);
            return _settings.GetType(tileBase);
        }

        private TileBase GetTile(Vector3Int vector3Int)
        {
            return _view.FloorTilemap.GetTile(vector3Int);
        }

        public bool HasTile(Vector3 point)
        {
            return _view.FloorTilemap.HasTile(WorldToCell(point));
        }

        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            return _view.FloorTilemap.WorldToCell(worldPosition);
        }

        public Vector3 CellToWorld(Vector3Int cell)
        {
            return _view.FloorTilemap.CellToWorld(cell);
        }

        public Vector3 GetCellCenterWorld(Vector3Int cell)
        {
            return _view.FloorTilemap.GetCellCenterWorld(cell);
        }

        public BoundsInt GetCellBounds()
        {
            return _view.FloorTilemap.cellBounds;
        }

        public bool ContainsCell(Vector3Int cell)
        {
            return _view.FloorTilemap.cellBounds.Contains(cell);
        }
    }
}