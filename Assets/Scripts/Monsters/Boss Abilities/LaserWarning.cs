using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class LaserWarning : MonoBehaviour
    {
        public SpriteRenderer laserRenderer;
        public float laserLength = 30f;
        private Coroutine blinkRoutine;

        public void Init(Vector2 position, LaserDirection direction, float interval, int count)
        {
            transform.position = position;
            transform.rotation = GetRotationFromDirection(direction);
            transform.localScale = new Vector3(laserLength, 1f, 1f);

            blinkRoutine = StartCoroutine(BlinkRoutine(interval, count));
        }

        private IEnumerator BlinkRoutine(float interval, int count)
        {
            for (int i = 0; i < count; i++)
            {
                laserRenderer.enabled = true;
                yield return new WaitForSeconds(0.5f * interval);
                laserRenderer.enabled = false;
                yield return new WaitForSeconds(0.5f * interval);
            }
            Destroy(gameObject);
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