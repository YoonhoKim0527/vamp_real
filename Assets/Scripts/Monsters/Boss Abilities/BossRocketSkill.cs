using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class BossRocketSkill : BossAbility
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject rocketPrefab;       // RocketProjectileSimple 포함
        [SerializeField] private GameObject explosionPrefab;    // 폭발 VFX

        [Header("Rocket Settings")]
        [SerializeField] private float cooldown = 2f;
        [SerializeField] private float rocketSpeed = 12f;
        [SerializeField] private float damage = 40f;
        [SerializeField] private float explosionRadius = 2.2f;
        [SerializeField] private LayerMask damageLayer;         // 폭발 시 데미지 대상

        [Header("Spawn")]
        [SerializeField] private Vector2 fallbackMuzzleLocalOffset = new Vector2(0.9f, -0.15f);
        [SerializeField] private float forwardSpawnOffset = 0.5f; // 시작점 살짝 앞

        private Transform muzzleTransform;
        private float nextAvailableTime = 0f;

        public override void Init(BossMonster monster, EntityManager entityManager, Character playerCharacter)
        {
            base.Init(monster, entityManager, playerCharacter);
            muzzleTransform = monster.transform.Find("muzzle"); // 자동 탐색
        }

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime || rocketPrefab == null) yield break;

            nextAvailableTime = Time.time + cooldown;
            active = true;
            monster.Stop();

            // 시작/목표 좌표 (발사 시점 스냅샷)
            Vector3 startPos = GetMuzzleWorldPosition();
            Vector3 targetPos = playerCharacter.transform.position;

            // 앞쪽으로 살짝 당겨 시작(겹침 방지, 물리 안 쓰지만 시각적으로 자연스러움)
            Vector2 dir = (targetPos - startPos).normalized;
            startPos += (Vector3)(dir * forwardSpawnOffset);

            // 로켓 생성 & 초기화 (※ 물리/콜라이더 안 씀)
            GameObject go = Instantiate(rocketPrefab, startPos, Quaternion.identity);
            var rocket = go.GetComponent<RocketProjectile>();
            rocket.Init(
                startPos: startPos,
                targetPos: targetPos,     // 고정 목표
                speed: rocketSpeed,
                damage: damage,
                explosionRadius: explosionRadius,
                damageLayer: damageLayer,
                explosionPrefab: explosionPrefab
            );

            // 폭발까지 대기 (원하면 즉시 return 해도 됨)
            while (!rocket.HasExploded) yield return null;

            active = false;
        }

        public override float Score() => Time.time >= nextAvailableTime ? 1f : 0f;

        private Vector3 GetMuzzleWorldPosition()
        {
            if (muzzleTransform != null) return muzzleTransform.position;

            var sr = monster.SpriteRenderer;
            Vector3 pivot = sr != null ? sr.bounds.center : monster.transform.position;

            bool facingLeft = sr != null && sr.flipX;
            Vector2 local = fallbackMuzzleLocalOffset;
            if (facingLeft) local.x = -local.x;

            return pivot + (Vector3)local;
        }
    }
}
