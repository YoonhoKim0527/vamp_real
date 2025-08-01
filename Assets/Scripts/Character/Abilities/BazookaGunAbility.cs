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
        private bool isCritical = false;

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

        private IEnumerator FireWaterBeam(Vector3? overrideCenter = null, float damageMultiplier = 1f, Color? ghostColor = null)
        {
            Vector3 basePos = overrideCenter ?? playerCharacter.CenterTransform.position;

            Vector3 leftPosition = basePos + Vector3.left * 4f;
            Vector3 rightPosition = basePos + Vector3.right * 4f;

            GameObject leftElephant = Instantiate(elephantLeftSprite, leftPosition, Quaternion.identity);
            GameObject rightElephant = Instantiate(elephantRightSprite, rightPosition, Quaternion.identity);

            // 유령 색상 적용
            if (ghostColor.HasValue)
            {
                var lsr = leftElephant.GetComponent<SpriteRenderer>();
                var rsr = rightElephant.GetComponent<SpriteRenderer>();
                if (lsr != null) lsr.color = ghostColor.Value;
                if (rsr != null) rsr.color = ghostColor.Value;
            }

            yield return new WaitForSeconds(1f);

            Vector3 beamStart = new Vector3(leftElephant.GetComponent<SpriteRenderer>().bounds.max.x, leftPosition.y, 0f);
            Vector3 beamEnd = new Vector3(rightElephant.GetComponent<SpriteRenderer>().bounds.min.x, rightPosition.y, 0f);
            Vector3 beamCenter = (beamStart + beamEnd) / 2f;

            float beamWidth = Vector3.Distance(beamStart, beamEnd);
            float beamHeight = waterBeamSprite.transform.localScale.y * 3f;

            GameObject waterBeam = Instantiate(waterBeamSprite, beamCenter, Quaternion.identity);
            waterBeam.transform.localScale = new Vector3(beamWidth, beamHeight, 1f);

            if (ghostColor.HasValue)
            {
                var sr = waterBeam.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = ghostColor.Value;
            }

            float elapsed = 0f;
            while (elapsed < beamDuration)
            {
                Collider2D[] hitMonsters = Physics2D.OverlapBoxAll(
                    beamCenter,
                    new Vector2(beamWidth, beamHeight),
                    0f,
                    monsterLayer
                );

                foreach (Collider2D collider in hitMonsters)
                {
                    Monster monster = collider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        float damageThisFrame = beamDamagePerSecond * Time.deltaTime * playerStats.attackPower * damageMultiplier;

                        bool isCrit = false;
                        if (Random.value < playerStats.criticalChance)
                        {
                            damageThisFrame *= (1 + playerStats.criticalDamage);
                            isCrit = true;
                        }

                        monster.TakeDamage(damageThisFrame, Vector2.zero, isCrit);
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(leftElephant);
            Destroy(rightElephant);
            Destroy(waterBeam);
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

        public override void MirrorActivate(float damageMultiplier, Vector3 spawnPosition, Color ghostColor)
        {
            if (!isEvolved) return;
            StartCoroutine(FireWaterBeam(spawnPosition, damageMultiplier, ghostColor));
        }
    }
}
