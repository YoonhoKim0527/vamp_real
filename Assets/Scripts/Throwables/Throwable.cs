using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Vampire
{
    public class Throwable : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer throwableSpriteRenderer;
        [SerializeField] protected SpriteRenderer shadowSpriteRenderer;
        [SerializeField] protected float maxDistance;
        [SerializeField] protected int maxBounceCount = 4;
        [SerializeField] protected float throwHeight = 1;
        [SerializeField] protected float bounciness = 0.5f;
        [SerializeField] protected float delayTime = 1;
        [SerializeField] protected float initialRotationSpeed = 2;
        protected float rotationSpeed;
        protected LayerMask targetLayer;
        protected float damage;
        protected float knockback;
        protected EntityManager entityManager;
        protected Character playerCharacter;
        protected Collider2D col;
        protected ZPositioner zPositioner;
        protected int throwableIndex;
        protected float throwTime;
        protected bool isCriticalHit = false; // ✅ 치명타 여부
        public UnityEvent<float> OnHitDamageable { get; private set; }
        public float ThrowTime => throwTime;
        public float Range => maxDistance;
        protected bool isCritical = false;

        protected virtual void Awake()
        {
            col = GetComponent<Collider2D>();
            zPositioner = gameObject.AddComponent<ZPositioner>();
        }

        public virtual void Init(EntityManager entityManager, Character playerCharacter)
        {
            this.entityManager = entityManager;
            this.playerCharacter = playerCharacter;
            zPositioner.Init(playerCharacter.transform);
        }

        public virtual void Setup(int throwableIndex, Vector2 position, float damage, float knockback, float timeInAir, LayerMask targetLayer)
        {
            transform.position = position;
            this.throwableIndex = throwableIndex;
            this.damage = damage;
            this.knockback = knockback;
            this.targetLayer = targetLayer;
            col.enabled = true;
            OnHitDamageable = new UnityEvent<float>();
            throwTime = ComputeThrowTime();
        }

        // ✅ 치명타 여부 주입 메서드
        public void SetCritical(bool critical)
        {
            isCriticalHit = critical;
        }

        public virtual void Throw(Vector2 toPosition, bool isCritical = false)
        {
            // 기존 로직 그대로
            Vector2 direction = (toPosition - (Vector2)transform.position);
            rotationSpeed = (direction.x > 0) ? initialRotationSpeed : -initialRotationSpeed;
            float throwDistance = direction.magnitude;
            if (throwDistance > maxDistance) throwDistance = maxDistance;
            direction.Normalize();

            // 🟥 isCritical 정보 저장하고 싶으면 멤버변수에 담기
            this.isCritical = isCritical;

            StartCoroutine(ThrowRoutine(direction, throwDistance));
        }

        public virtual IEnumerator ThrowRoutine(Vector2 direction, float throwDistance)
        {
            float distance = 0;
            int bounceCount = 0;
            Vector3 initialPosition = transform.position;
            float vy = Mathf.Sqrt(2 * PhysicsConstants.g * throwHeight);
            float initialLinearVelocity = InitialVelocityForThrowDistance(throwDistance, vy);
            float linearSpeed = initialLinearVelocity;
            float y = 0;
            while (distance < throwDistance && linearSpeed > 0)
            {
                Vector3 linearPosition = initialPosition + distance * (Vector3)direction;

                vy -= PhysicsConstants.g * Time.deltaTime;
                y += vy * Time.deltaTime;
                if (y < 0)
                {
                    y = 0;
                    vy = -vy * bounciness;
                    bounceCount++;
                    linearSpeed = (initialLinearVelocity * (maxBounceCount - bounceCount)) / maxBounceCount;
                    rotationSpeed *= bounciness;
                }
                transform.position = linearPosition + Vector3.up * y;
                zPositioner.ManuallySetZByY(linearPosition.y);

                if (bounceCount >= maxBounceCount)
                    break;

                throwableSpriteRenderer.transform.RotateAround(throwableSpriteRenderer.transform.position, Vector3.back, Time.deltaTime * 100 * rotationSpeed);

                shadowSpriteRenderer.transform.position = linearPosition;
                shadowSpriteRenderer.transform.rotation = Quaternion.identity;

                distance += linearSpeed * Time.deltaTime;
                yield return null;
            }
            zPositioner.AutomaticallySetZ();
            float t = 0;
            float rollingSpeed = 1.0f / maxBounceCount * initialLinearVelocity;
            while (t < delayTime)
            {
                Vector3 linearPosition = initialPosition + distance * (Vector3)direction;
                transform.position = linearPosition;
                shadowSpriteRenderer.transform.position = linearPosition;
                shadowSpriteRenderer.transform.rotation = Quaternion.identity;
                t += Time.deltaTime;
                distance += rollingSpeed * Time.deltaTime;
                throwableSpriteRenderer.transform.RotateAround(throwableSpriteRenderer.transform.position, Vector3.back, Time.deltaTime * 100 * rotationSpeed);
                rollingSpeed *= 0.95f;
                rotationSpeed *= 0.99f;
                yield return null;
            }
            Explode();
        }

        private float InitialVelocityForThrowDistance(float throwDistance, float vy)
        {
            float H = 1;
            for (int i = 1; i <= maxBounceCount; i++)
            {
                H += Mathf.Pow(bounciness, i) * (1 - ((float)i) / maxBounceCount);
            }
            return PhysicsConstants.g * throwDistance / (2 * vy * H);
        }

        protected float ComputeThrowTime()
        {
            float vy = Mathf.Sqrt(2 * PhysicsConstants.g * throwHeight);
            float t = 0;
            for (int i = 1; i <= maxBounceCount; i++)
            {
                t += Mathf.Pow(bounciness, i) * 2 * vy / PhysicsConstants.g;
            }
            return t;
        }

        protected virtual void Explode()
        {
            // ✅ 폭발시 치명타 여부까지 전달
            entityManager.SpawnDamageText(transform.position, damage, isCriticalHit);
            DestroyThrowable();
        }

        protected virtual void DestroyThrowable()
        {
            StartCoroutine(DestroyThrowableAnimation());
        }

        protected IEnumerator DestroyThrowableAnimation()
        {
            throwableSpriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.0f);
            throwableSpriteRenderer.enabled = true;
            entityManager.DespawnThrowable(throwableIndex, this);
        }
    }
}
