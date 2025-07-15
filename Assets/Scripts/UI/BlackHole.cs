using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class BlackHole : MonoBehaviour
    {
        [SerializeField] float duration = 5f;
        [SerializeField] float pullForce = 10f;
        [SerializeField] float pullRadius = 5f;
        [SerializeField] LayerMask pullTargetLayer;

        HashSet<Rigidbody2D> pullTargets = new();

        void OnEnable()
        {
            StartCoroutine(DeactivateAfterDuration());
        }

        void FixedUpdate()
        {
            foreach (var target in pullTargets)
            {
                if (target == null) continue;

                Vector2 direction = (transform.position - target.transform.position).normalized;
                float distance = Vector2.Distance(transform.position, target.transform.position);
                float strength = 1 - Mathf.Clamp01(distance / pullRadius);

                target.AddForce(direction * pullForce * strength, ForceMode2D.Force);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & pullTargetLayer) != 0)
            {
                Rigidbody2D rb = other.attachedRigidbody;
                if (rb != null && !pullTargets.Contains(rb))
                    pullTargets.Add(rb);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & pullTargetLayer) != 0)
            {
                Rigidbody2D rb = other.attachedRigidbody;
                if (rb != null)
                    pullTargets.Remove(rb);
            }
        }

        IEnumerator DeactivateAfterDuration()
        {
            yield return new WaitForSeconds(duration);
            gameObject.SetActive(false);
        }
    }
}
