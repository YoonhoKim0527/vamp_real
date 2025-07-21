using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class BazookaGunAbility : GunAbility
    {
        [Header("Bazooka Gun Stats")]
        [SerializeField] protected GameObject bazookaGun;
        [SerializeField] protected Transform launchTransform;
        [SerializeField] protected ParticleSystem launchParticles;
        [SerializeField] protected UpgradeableAOE explosionAOE;
        [SerializeField] protected Vector2 hoverOffset;
        [SerializeField] protected float targetRadius = 5;

        [Header("Water Beam Settings")]
        [SerializeField] private GameObject elephantLeftSprite;
        [SerializeField] private GameObject elephantRightSprite;
        [SerializeField] private GameObject waterBeamSprite;
        [SerializeField] private float beamDuration = 2f;
        [SerializeField] private float beamDamagePerSecond = 500f;
        [SerializeField] private LayerMask monsterLayer;

        private Vector2 currHoverOffset;
        private float theta = 0;
        private bool isEvolved = false;

        protected override void Update()
        {
            base.Update();

            if (isEvolved)
            {
                bazookaGun.SetActive(false);
                return;
            }

            currHoverOffset = hoverOffset + Vector2.up * Mathf.Sin(Time.time * 5) * 0.1f;
            bazookaGun.transform.position = (Vector2)playerCharacter.CenterTransform.position + currHoverOffset;
        }

        protected override void LaunchProjectile()
        {
            if (level >= 2 && !isEvolved)
            {
                isEvolved = true;
                StartCoroutine(WaterBeamAttackLoop());
            }
            else
            {
                StartCoroutine(LaunchProjectileAnimation());
            }
        }

        private IEnumerator WaterBeamAttackLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f); // 10초마다 발동
                StartCoroutine(FireWaterBeam());
            }
        }

        private IEnumerator FireWaterBeam()
        {
            Debug.Log("[BazookaGun] 🐘 코끼리 물대포 발사!");

            // 🐘 1. 양쪽 코끼리 생성
            Vector3 leftPosition = playerCharacter.CenterTransform.position + Vector3.left * 4f;
            Vector3 rightPosition = playerCharacter.CenterTransform.position + Vector3.right * 4f;

            GameObject leftElephant = Instantiate(elephantLeftSprite, leftPosition, Quaternion.identity);
            GameObject rightElephant = Instantiate(elephantRightSprite, rightPosition, Quaternion.identity);

            // SpriteRenderer 가져오기
            SpriteRenderer leftSR = leftElephant.GetComponent<SpriteRenderer>();
            SpriteRenderer rightSR = rightElephant.GetComponent<SpriteRenderer>();

            yield return new WaitForSeconds(1f); // 코끼리 등장 후 1초 대기

            // 💧 2. 물대포 생성
            Vector3 beamStart = new Vector3(leftSR.bounds.max.x, leftPosition.y, 0f);  // 왼쪽 코끼리 오른쪽 끝
            Vector3 beamEnd = new Vector3(rightSR.bounds.min.x, rightPosition.y, 0f); // 오른쪽 코끼리 왼쪽 끝

            Vector3 beamCenter = (beamStart + beamEnd) / 2f;
            float beamWidth = Vector3.Distance(beamStart, beamEnd);
            float beamHeight = waterBeamSprite.transform.localScale.y * 3f; // 두께 3배

            GameObject waterBeam = Instantiate(waterBeamSprite, beamCenter, Quaternion.identity);
            waterBeam.transform.localScale = new Vector3(
                beamWidth,
                beamHeight,
                1f
            );

            // 💥 3. 물대포 범위 내 몬스터 데미지
            float elapsed = 0f;
            while (elapsed < beamDuration)
            {
                Collider2D[] hitMonsters = Physics2D.OverlapBoxAll(
                    beamCenter,
                    new Vector2(beamWidth, beamHeight),
                    0f,
                    monsterLayer
                );

                Debug.Log($"[BazookaGun] 🌊 감지 몬스터 수: {hitMonsters.Length}");

                foreach (Collider2D collider in hitMonsters)
                {
                    Monster monster = collider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        // ✅ CharacterStatBlueprint 기반 데미지 계산
                        float damageThisFrame = beamDamagePerSecond * Time.deltaTime * playerStats.attackPower;

                        // ✅ 치명타 확률 적용
                        if (Random.value < playerStats.criticalChance)
                        {
                            damageThisFrame *= (1 + playerStats.criticalDamage);
                            Debug.Log("[BazookaGun] 💥 Critical Hit!");
                        }

                        monster.TakeDamage(damageThisFrame, Vector2.zero);
                        Debug.Log($"[BazookaGun] 🐘 {monster.name}에게 {damageThisFrame:F1} 데미지 적용");
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(leftElephant);
            Destroy(rightElephant);
            Destroy(waterBeam);

            Debug.Log("[BazookaGun] 🐘 물대포 종료");
        }

        private IEnumerator LaunchProjectileAnimation()
        {
            ISpatialHashGridClient targetEntity = entityManager.Grid.FindClosestInRadius(bazookaGun.transform.position, targetRadius);

            Vector2 launchDirection = targetEntity == null
                ? Random.insideUnitCircle.normalized
                : (targetEntity.Position - (Vector2)bazookaGun.transform.position).normalized;

            float targetTheta = Vector2.SignedAngle(Vector2.right, launchDirection);
            float initialTheta = bazookaGun.transform.eulerAngles.z;

            float t = 0;
            float tMax = 1 / firerate.Value * 0.45f;
            while (t < tMax)
            {
                float tScaled = t / tMax;
                if (targetEntity != null)
                {
                    launchDirection = (targetEntity.Position - (Vector2)bazookaGun.transform.position).normalized;
                    targetTheta = Vector2.SignedAngle(Vector2.right, launchDirection);
                }
                bazookaGun.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(initialTheta, targetTheta, tScaled));
                t += Time.deltaTime;
                yield return null;
            }

            bazookaGun.transform.rotation = Quaternion.Euler(0, 0, targetTheta);

            // ✅ 발사체 생성 (CharacterStatBlueprint 반영)
            float projectileDamage = playerStats.attackPower * damage.Value;

            // ✅ 치명타 확률 적용
            if (Random.value < playerStats.criticalChance)
            {
                projectileDamage *= (1 + playerStats.criticalDamage);
                Debug.Log("[BazookaGun] 💥 Critical Projectile!");
            }

            ExplosiveProjectile projectile = (ExplosiveProjectile)entityManager.SpawnProjectile(
                projectileIndex,
                launchTransform.position,
                projectileDamage,
                knockback.Value * (1 + playerStats.defense * 0.1f), // ✅ 넉백 강화
                speed.Value,
                monsterLayer
            );
            projectile.SetupExplosion(projectileDamage, explosionAOE.Value, knockback.Value);
            projectile.OnHitDamageable.AddListener(playerCharacter.OnDealDamage.Invoke);

            launchParticles.Play();

            // 발사체 날리기
            projectile.Launch(launchDirection);
        }
    }
}
