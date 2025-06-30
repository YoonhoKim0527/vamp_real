using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class RadialBlastAbility : ProjectileAbility
    {
        [SerializeField] private Sprite effectSprite; // 큰 이펙트 이미지
        [SerializeField] private Sprite redEyeEffectSprite;         // 3레벨 이상 효과 이미지

        [SerializeField] private UpgradeableProjectileCount projectileCount;
        [SerializeField] private Sprite redEyeProjectileSprite;     // 3레벨 이상 시 발사체

        [SerializeField] private AudioClip normalClip;
        [SerializeField] private AudioClip evolvedClip;
        private AudioSource audioSource;

        private float timeSinceLastCast;

        protected override void Use()
        {
            base.Use();
            timeSinceLastCast = cooldown.Value;

            if (CrossSceneData.ExtraProjectile && projectileCount != null)
            {
                projectileCount.ForceAdd(1);  
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
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
            Sprite spriteToUse = level >= 3 ? redEyeEffectSprite : effectSprite;

            // 1. 이펙트 이미지 표시 (잠시 후 제거)
            GameObject effect = new GameObject("RadialBlastEffect");
            SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
            sr.sprite = spriteToUse;
            sr.sortingOrder = 1000;
            effect.transform.position = playerCharacter.CenterTransform.position;
            effect.transform.localScale = Vector3.one * 2f; // 크게

            // 🔊 AudioSource 생성 및 blastSound 재생
            if (audioSource != null)
            {
                audioSource.volume = 1f; // 조정 가능
                audioSource.clip = level >= 3 ? evolvedClip : normalClip;
                audioSource.Play();
            }

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

                // 🔴 3레벨 이상이면 발사체 스프라이트 바꾸기
                if (level >= 3 && redEyeProjectileSprite != null)
                {
                    var srProj = p.GetComponentInChildren<SpriteRenderer>();
                    if (srProj != null)
                        srProj.sprite = redEyeProjectileSprite;
                }

                p.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);
                p.Launch(dir);
            }
        }
    }
}
