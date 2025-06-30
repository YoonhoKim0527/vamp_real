using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class RadialBlastAbility : ProjectileAbility
    {
        [SerializeField] private Sprite effectSprite; // 큰 이펙트 이미지
        [SerializeField] private UpgradeableProjectileCount projectileCount;
        [SerializeField] private AudioClip blastSound;

        private float timeSinceLastCast;

        protected override void Use()
        {
            base.Use();
            timeSinceLastCast = cooldown.Value;
        }

        protected override void Update()
        {
            base.Update();
            timeSinceLastCast += Time.deltaTime;
            if (timeSinceLastCast >= cooldown.Value)
            {
                StartCoroutine(CastRadialBlast());
                timeSinceLastCast = 0f;
            }
        }

        protected override void Attack()
        {
            // 부모에서 호출될 수 있으므로 빈 override로 무력화
        }

        private IEnumerator CastRadialBlast()
        {
            // 1. 이펙트 이미지 표시 (잠시 후 제거)
            GameObject effect = new GameObject("RadialBlastEffect");
            SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
            sr.sprite = effectSprite;
            sr.sortingOrder = 1000;
            effect.transform.position = playerCharacter.CenterTransform.position;
            effect.transform.localScale = Vector3.one * 4f; // 크게

            // 🔊 AudioSource 생성 및 blastSound 재생
            AudioSource audioSource = effect.AddComponent<AudioSource>();
            audioSource.clip = blastSound;
            audioSource.playOnAwake = false;
            audioSource.volume = 1f; // 조정 가능
            audioSource.Play();

            yield return new WaitForSeconds(1.5f);
            Destroy(effect);

            // 2. 투사체 12방향 발사
            for (int i = 0; i < projectileCount.Value; i++)
            {
                float angle = 360f / projectileCount.Value * i;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
                Projectile p = entityManager.SpawnProjectile(
                    projectileIndex,
                    playerCharacter.CenterTransform.position,
                    damage.Value,
                    knockback.Value,
                    speed.Value,
                    monsterLayer
                );
                p.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
                p.Launch(dir);
            }
        }
    }
}
