using Core.ServiceLocator;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Grid
{
    public class GridView : MonoView
    {
        [field: SerializeField] public UnityEngine.Grid Grid { get; private set; }
        [field: SerializeField] public Tilemap FloorTilemap { get; private set; }
    }
}