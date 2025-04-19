using Core.ServiceLocator;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.CoreGame.Grid
{
    public class GridView : MonoView
    {
        [field: SerializeField] public UnityEngine.Grid Grid { get; private set; }
        [field: SerializeField] public Tilemap FloorTilemap { get; private set; }
    }
}