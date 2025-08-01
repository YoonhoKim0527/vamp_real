using UnityEngine;

namespace Vampire
{
    public class RealLaser : MonoBehaviour
    {
        public float duration = 1.5f;
        public float laserLength = 30f;
        public float damage = 50f;
        public SpriteRenderer laserRenderer;
        public LayerMask playerLayer;

        public void Init(Vector2 position, LaserDirection direction)
        {
            transform.position = position;
            transform.rotation = GetRotationFromDirection(direction);
            transform.localScale = new Vector3(laserLength, 1f, 1f);

            Invoke(nameof(DoDamage), 0.1f);
            Destroy(gameObject, duration);
        }

        private void DoDamage()
        {
            Vector2 dir = transform.right;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, laserLength, playerLayer);

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent(out Character player))
                {
                    player.TakeDamage(damage);
                }
            }
        }

        private Quaternion GetRotationFromDirection(LaserDirection dir)
        {
            return dir switch
            {
                LaserDirection.Horizontal => Quaternion.identity,
                LaserDirection.Vertical => Quaternion.Euler(0, 0, 90),
                LaserDirection.DiagonalRight => Quaternion.Euler(0, 0, 45),
                LaserDirection.DiagonalLeft => Quaternion.Euler(0, 0, -45),
                _ => Quaternion.identity
            };
        }
    }
}