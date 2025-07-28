using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

namespace Vampire
{
    public class BlackHoleProjectile : Projectile
    {
        [Header("Black Hole Settings")]
        float startRadius = 1f;
        float endRadius;
        float pullForce;
        float explosionDelay;
        float knock;
        float travelDistance;
        float tickInterval = 0.5f;

        bool hasActivated = false;

        Vector3 targetPosition;
        Vector3 blackHoleCenter;

        Rigidbody2D rb;
        HashSet<Rigidbody2D> affectedBodies = new HashSet<Rigidbody2D>();

        public void Init(
            Vector2 dir,
            Character player,
            LayerMask targetLayer,
            bool isCrit,
            float delay,
            float pull,
            float radius,
            float knockback,
            ParticleSystem effect, // unused
            float damage,
            float travelDistance
        )
        {
            direction = dir.normalized;
            playerCharacter = player;
            this.targetLayer = targetLayer;
            this.isCritical = isCrit;
            this.explosionDelay = delay;
            this.pullForce = pull;
            this.endRadius = radius;
            this.knock = knockback;
            this.damage = damage;
            this.travelDistance = travelDistance;

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                Debug.LogWarning("❌ Rigidbody2D missing on BlackHoleProjectile");

            targetPosition = player.CenterTransform.position + (Vector3)(direction * travelDistance);

            StartCoroutine(TravelThenActivate());
        }

        protected override void OnTriggerEnter2D(Collider2D other) {
            
        }
        IEnumerator TravelThenActivate()
        {
            float moveSpeed = speed;
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            ActivateBlackHole();
        }

        void ActivateBlackHole()
        {
            hasActivated = true;
            Debug.Log("🌀 BlackHole Activated");

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            col.enabled = false;
            transform.localScale = Vector3.one * startRadius;
            blackHoleCenter = targetPosition;

            StartCoroutine(BlackHoleSequence());
        }

        IEnumerator BlackHoleSequence()
        {
            float timeElapsed = 0f;
            float tickTimer = 0f;

            while (timeElapsed < explosionDelay)
            {
                timeElapsed += Time.deltaTime;
                tickTimer += Time.deltaTime;

                float scale = Mathf.Lerp(startRadius, endRadius, timeElapsed / explosionDelay);
                transform.localScale = Vector3.one * scale;
                float currentRadius = transform.lossyScale.x * 0.5f;

                Collider2D[] hits = Physics2D.OverlapCircleAll(blackHoleCenter, currentRadius, targetLayer);

                foreach (var hit in hits)
                {
                    var monsterRoot = hit.GetComponentInParent<Monster>();
                    if (monsterRoot == null) continue;

                    var targetTransform = monsterRoot.transform;
                    Vector2 dirToCenter = (blackHoleCenter - targetTransform.position).normalized;
                    float dynamicPull = pullForce * (currentRadius / endRadius);

                    if (monsterRoot.TryGetComponent(out Rigidbody2D hitRb))
                    {
                        if (!affectedBodies.Contains(hitRb))
                        {
                            affectedBodies.Add(hitRb);
                        }

                        hitRb.AddForce(dirToCenter * pullForce, ForceMode2D.Force);
                    }
                    else
                    {
                        targetTransform.position += (Vector3)(dirToCenter * dynamicPull * Time.deltaTime);
                    }
                }

                // DoT: 일정 주기마다 처리
                if (tickTimer >= tickInterval)
                {
                    tickTimer = 0f;
                    foreach (var hit in hits)
                    {
                        var monsterRoot = hit.GetComponentInParent<Monster>();
                        if (monsterRoot == null) continue;

                        if (monsterRoot.TryGetComponent(out IDamageable d))
                        {
                            float tickDamage = damage * 0.2f;
                            d.TakeDamage(tickDamage, Vector2.zero, isCritical);
                            OnHitDamageable.Invoke(tickDamage);
                            entityManager.SpawnDamageText(monsterRoot.transform.position, tickDamage, isCritical);
                        }
                    }
                }

                yield return null;
            }

            Explode();
        }

        void Explode()
        {
            float radius = transform.lossyScale.x * 0.5f;
            Debug.Log("💥 BlackHole Exploding with radius: " + radius);

            Collider2D[] hits = Physics2D.OverlapCircleAll(blackHoleCenter, radius, targetLayer);
            foreach (var hit in hits)
            {
                var monsterRoot = hit.GetComponentInParent<Monster>();
                if (monsterRoot == null) continue;

                if (monsterRoot.TryGetComponent(out IDamageable d))
                {
                    d.TakeDamage(damage, Vector2.zero, isCritical);
                    OnHitDamageable.Invoke(damage);
                    entityManager.SpawnDamageText(monsterRoot.transform.position, damage, isCritical);
                }
            }

            DestroyProjectile(); // ✅ 블랙홀 사라짐
        }

        protected override void HitNothing() => ActivateBlackHole();
    }
}
