using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class BossSpinAttack : BossAbility
    {
        [Header("Spin Settings")]
        public float spinDuration = 3f;
        public float spinDamageInterval = 0.2f;
        public float spinDamage = 10f;
        public float spinRadius = 1.5f;
        public float speedMultiplier = 3f; // 이동속도 증가 배율
        public float scaleMultiplier = 1.5f;
        public float spinSpeed = 3000f; // 초당 회전 각도

        [Header("Sprite")]
        [SerializeField] private Sprite spinSprite;
        private Sprite originalSprite;
        private SpriteRenderer spriteRenderer;

        private float nextAvailableTime = 0f;
        private Vector3 originalScale;
        private float originalSpeed;
        private float originalAcceleration;
        private Quaternion originalRotation;

        [Header("Spin Acceleration Multiplier")]
        public float accelerationMultiplier = 10f; // 회전 중 가속도 증가

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime)
                yield break;

            nextAvailableTime = Time.time + 10f;
            active = true;

            // 초기 상태 저장
            originalScale = monster.transform.localScale;
            originalRotation = monster.transform.rotation;
            originalSpeed = monster.moveSpeed;
            originalAcceleration = monster.Blueprint.acceleration;

            if (spriteRenderer == null)
                spriteRenderer = monster.GetComponentInChildren<SpriteRenderer>();
            originalSprite = spriteRenderer.sprite;
            if (spinSprite != null)
                spriteRenderer.sprite = spinSprite;

            // 상태 변경
            monster.transform.localScale = originalScale * scaleMultiplier;
            monster.moveSpeed *= speedMultiplier;
            monster.Blueprint.acceleration *= accelerationMultiplier;

            // 이동 및 피해 처리 코루틴 실행
            Coroutine spinMotion = monster.StartCoroutine(SpinMotion());
            Coroutine spinDamage = monster.StartCoroutine(SpinDamageRoutine());

            yield return new WaitForSeconds(spinDuration);

            // 종료 복구
            monster.StopCoroutine(spinMotion);
            monster.StopCoroutine(spinDamage);

            monster.transform.localScale = originalScale;
            monster.moveSpeed = originalSpeed;
            monster.Blueprint.acceleration = originalAcceleration;
            monster.transform.rotation = originalRotation;
            monster.Rigidbody.velocity = Vector2.zero;

            if (spinSprite != null && originalSprite != null)
                spriteRenderer.sprite = originalSprite;

            active = false;
        }

        private IEnumerator SpinMotion()
        {
            while (true)
            {
                // 플레이어 방향 계산
                Vector2 dir = (playerCharacter.transform.position - monster.transform.position).normalized;

                // ✅ Move 사용 (가속도 증가된 상태)
                monster.Move(dir, Time.deltaTime);

                // ✅ 빠른 회전
                monster.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator SpinDamageRoutine()
        {
            while (true)
            {
                DoSpinDamage();
                yield return new WaitForSeconds(spinDamageInterval);
            }
        }

        private void DoSpinDamage()
        {
            Collider2D[] hit = Physics2D.OverlapCircleAll(monster.transform.position, spinRadius);
            foreach (var collider in hit)
            {
                if (collider.TryGetComponent<Character>(out var player))
                {
                    Vector2 knockDir = (player.transform.position - monster.transform.position).normalized;
                    player.TakeDamage(spinDamage, knockDir * 3f);
                }
            }
        }

        public override float Score()
        {
            return Time.time >= nextAvailableTime ? 1f : 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (monster != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(monster.transform.position, spinRadius);
            }
        }
    }
}
