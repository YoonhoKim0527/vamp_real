using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Vampire
{
    public class Boomerang : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer boomerangSpriteRenderer;
        [SerializeField] protected float rotationSpeed = 2;
        [SerializeField] protected float damageDelay = 0.1f;

        protected Vector2 initialScale;
        protected float throwTime = 1;
        protected float maxDistance;
        protected float radius;
        protected LayerMask targetLayer;
        protected float damage;
        protected float knockback;

        protected EntityManager entityManager;
        protected Character playerCharacter;
        protected ZPositioner zPositioner;
        protected int boomerangIndex;

        protected TrailRenderer trailRenderer = null;
        private bool isCriticalHit = false;

        public UnityEvent<float> OnHitDamageable { get; private set; }
        public float Range => maxDistance;

        protected virtual void Awake()
        {
            radius = Mathf.Max(boomerangSpriteRenderer.bounds.size.x, boomerangSpriteRenderer.bounds.size.y) / 2;
            initialScale = boomerangSpriteRenderer.transform.localScale;
            zPositioner = gameObject.AddComponent<ZPositioner>();
            TryGetComponent<TrailRenderer>(out trailRenderer);
        }

        public virtual void Init(EntityManager entityManager, Character playerCharacter)
        {
            this.entityManager = entityManager;
            this.playerCharacter = playerCharacter;

            if (zPositioner == null)
                zPositioner = gameObject.AddComponent<ZPositioner>();

            zPositioner.Init(playerCharacter.transform);
        }

        public virtual void Setup(int boomerangIndex, Vector2 position, float damage, float knockback, float throwDistance, float throwTime, LayerMask targetLayer)
        {
            transform.position = position;
            trailRenderer?.Clear();
            this.boomerangIndex = boomerangIndex;
            this.damage = damage;
            this.knockback = knockback;
            this.targetLayer = targetLayer;
            this.maxDistance = throwDistance;
            this.throwTime = throwTime;
            OnHitDamageable = new UnityEvent<float>();
        }

        public void InitCritical(bool isCritical)
        {
            this.isCriticalHit = isCritical;
        }

        public virtual void Throw(Transform returnTransform, Vector2 toPosition)
        {
            Vector2 direction = (toPosition - (Vector2)transform.position).normalized;
            StartCoroutine(ThrowRoutine(returnTransform, direction, maxDistance));
        }

        public virtual IEnumerator ThrowRoutine(Transform returnTransform, Vector2 direction, float throwDistance)
        {
            Dictionary<GameObject, float> hitMonsterTimes = new Dictionary<GameObject, float>();
            Vector2 prevPosition = transform.position;
            Vector2 a = transform.position;
            Vector2 c = (Vector2)transform.position + direction * throwDistance;

            float t = 0;
            while (t < 1)
            {
                transform.position = Vector2.Lerp(a, c, EasingUtils.EaseOutQuad(t));

                Vector2 circleCastDir = (Vector2)transform.position - prevPosition;
                RaycastHit2D[] raycastHits = Physics2D.CircleCastAll(prevPosition, radius, circleCastDir.normalized, circleCastDir.magnitude, targetLayer);
                foreach (RaycastHit2D hit in raycastHits)
                {
                    GameObject hitObj = hit.collider.gameObject;
                    if (!hitMonsterTimes.ContainsKey(hitObj) || hitMonsterTimes[hitObj] > damageDelay)
                    {
                        hitMonsterTimes[hitObj] = 0.0f;
                        IDamageable damageable = hitObj.GetComponentInParent<IDamageable>();
                        float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage;
                        damageable.TakeDamage(totalDamage, circleCastDir.normalized * knockback, isCriticalHit);
                        OnHitDamageable.Invoke(totalDamage);
                    }
                }

                prevPosition = transform.position;
                boomerangSpriteRenderer.transform.RotateAround(boomerangSpriteRenderer.transform.position, Vector3.back, Time.deltaTime * 100 * rotationSpeed);
                boomerangSpriteRenderer.transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t * 5);

                foreach (var key in hitMonsterTimes.Keys.ToArray())
                {
                    hitMonsterTimes[key] += Time.deltaTime;
                }

                t += Time.deltaTime / throwTime;
                yield return null;
            }

            t = 0;
            while (t < 1)
            {
                Vector3 returnTarget = (playerCharacter != null && playerCharacter.CenterTransform != null)
                    ? playerCharacter.CenterTransform.position
                    : returnTransform != null ? returnTransform.position : transform.position;

                transform.position = Vector2.Lerp(c, returnTarget, EasingUtils.EaseInQuad(t));

                Vector2 circleCastDir = (Vector2)transform.position - prevPosition;
                RaycastHit2D[] raycastHits = Physics2D.CircleCastAll(prevPosition, radius, circleCastDir.normalized, circleCastDir.magnitude, targetLayer);
                foreach (RaycastHit2D hit in raycastHits)
                {
                    GameObject hitObj = hit.collider.gameObject;
                    if (!hitMonsterTimes.ContainsKey(hitObj) || hitMonsterTimes[hitObj] > damageDelay)
                    {
                        hitMonsterTimes[hitObj] = 0.0f;
                        IDamageable damageable = hitObj.GetComponentInParent<IDamageable>();
                        float totalDamage = playerCharacter.Stats.GetTotalDamage() * damage;
                        damageable.TakeDamage(totalDamage, -circleCastDir.normalized * knockback, isCriticalHit);
                        OnHitDamageable.Invoke(totalDamage);
                    }
                }

                prevPosition = transform.position;
                boomerangSpriteRenderer.transform.RotateAround(boomerangSpriteRenderer.transform.position, Vector3.back, Time.deltaTime * 100 * rotationSpeed);
                boomerangSpriteRenderer.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, (t - 0.8f) * 5);

                foreach (var key in hitMonsterTimes.Keys.ToArray())
                {
                    hitMonsterTimes[key] += Time.deltaTime;
                }

                t += Time.deltaTime / throwTime;
                yield return null;
            }

            DestroyThrowable();
        }

        protected virtual void DestroyThrowable()
        {
            StartCoroutine(DestroyThrowableAnimation());
        }

        protected IEnumerator DestroyThrowableAnimation()
        {
            boomerangSpriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.0f);
            boomerangSpriteRenderer.enabled = true;
            entityManager.DespawnBoomerang(boomerangIndex, this);
        }
    }
}
