using Unity.Mathematics;
using UnityEngine;

namespace Core.Extensions
{
    public static class VectorExtension
    {
        public static Vector2 AsVector2(this float2 vector) => new Vector2(vector.x, vector.y);

        #region to float2
        public static float2 AsFloat2(this Vector2 vector) => new float2(vector.x, vector.y);
        public static float2 AsFloat2(this Vector2Int vector) => new float2(vector.x, vector.y);
        public static float2 AsFloat2(this Vector3 vector) => new float2(vector.x, vector.y);
        public static float2 AsFloat2(this Vector3Int vector) => new float2(vector.x, vector.y);

        #endregion
    }
}