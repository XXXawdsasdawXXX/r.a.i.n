using Code.CoreGame.Entities.Params;
using UnityEngine;

namespace Code.CoreGame.Entities
{
    public class Obstacle : MonoBehaviour
    {
        private void OnCollisionExit2D(Collision2D other)
        {
            
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent(out Health health))
            {
                health.UpdateHealth(-5);
            }
        }
    }
}