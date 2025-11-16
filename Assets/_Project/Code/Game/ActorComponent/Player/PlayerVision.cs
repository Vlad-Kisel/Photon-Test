using Fusion;
using UnityEngine;

namespace _Project.Code.Infrastructure.Network
{
    public class PlayerVision : NetworkBehaviour
    {
        private readonly Color _gizmoColor = Color.cyan;
        
        [field: SerializeField] public Vector2 Extents {get; private set;}

        public bool IsPointInsideVision(Vector3 point)
        {
            var deltaX = point.x - transform.position.x;
            var deltaZ = point.z - transform.position.z;

            return Mathf.Abs(deltaX) <= Extents.x && Mathf.Abs(deltaZ) <= Extents.y;
        }
        
        private void OnDrawGizmos()
        {
            if (Extents == Vector2.zero)
                return;

            var center = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            
            DrawRect(center, Extents, _gizmoColor);
        }

        private void DrawRect(Vector3 center, Vector2 extents, Color color)
        {
            Gizmos.color = color;

            var a = center + new Vector3(-extents.x, 0, -extents.y);
            var b = center + new Vector3(-extents.x, 0,  extents.y);
            var c = center + new Vector3( extents.x, 0,  extents.y);
            var d = center + new Vector3( extents.x, 0, -extents.y);

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }
}