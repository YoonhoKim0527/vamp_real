using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class BossSwordRainSkill : BossAbility
    {
        [Header("Prefab and Spawn Settings")]
        [SerializeField] private GameObject swordPrefab;
        [SerializeField] private float launchDelay = 3f;
        [SerializeField] private float fireInterval = 10f;
        [SerializeField] private float launchSpeed = 10f;

        private float nextAvailableTime = 0f;

        private readonly Vector2[] swordOffsets = new Vector2[]
        {
            new Vector2(-2f, 0.5f),
            new Vector2(-1f, 1.2f),
            new Vector2(0f, 2f),
            new Vector2(1f, 1.2f),
            new Vector2(2f, 0.5f),
        };

        private struct SwordLaunchData
        {
            public GameObject sword;
            public Vector2 direction;
        }

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime || swordPrefab == null)
                yield break;

            nextAvailableTime = Time.time + fireInterval;
            active = true;

            List<SwordLaunchData> swords = new List<SwordLaunchData>();

            // 1. instantiate swords facing upward
            foreach (Vector2 offset in swordOffsets)
            {
                Vector2 spawnPos = (Vector2)monster.transform.position + offset;
                GameObject sword = Instantiate(swordPrefab, spawnPos, Quaternion.Euler(0, 0, 0), monster.transform); // 위쪽 바라보게 생성

                // 나중에 회전 및 발사할 방향 미리 계산
                Vector2 randomDir = Random.insideUnitCircle.normalized;

                swords.Add(new SwordLaunchData { sword = sword, direction = randomDir });
            }

            // 2. 보스는 대기 시간 동안 추적 이동 가능
            float waitTimer = 0f;
            while (waitTimer < launchDelay)
            {
                Vector2 moveDir = (playerCharacter.transform.position - monster.transform.position).normalized;
                monster.Move(moveDir, Time.deltaTime);

                waitTimer += Time.deltaTime;
                yield return null;
            }

            // 3. 발사 전 회전 + 발사
            foreach (var data in swords)
            {
                data.sword.transform.parent = null;

                // ✅ 방향 회전 (칼끝이 진행 방향을 향하게)
                float angle = Mathf.Atan2(data.direction.y, data.direction.x) * Mathf.Rad2Deg - 90f;
                data.sword.transform.rotation = Quaternion.Euler(0, 0, angle);

                // ✅ velocity 설정
                if (data.sword.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.velocity = data.direction * launchSpeed;
                }
            }

            active = false;
        }

        public override float Score()
        {
            return Time.time >= nextAvailableTime ? 1f : 0f;
        }
    }
}
