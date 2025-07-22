using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class FlameWaveAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] int waveCount = 3;
        [SerializeField] float waveSpacing = 0.2f;
        [SerializeField] float spreadAngle = 15f; // 좌우 퍼짐 각도

        protected override void TriggerAbility()
        {
            if (boss == null) return;

            Vector3 forward = (boss.position - transform.position).normalized;

            // 중심, 왼쪽, 오른쪽 방향 계산
            Vector3[] directions = new Vector3[3];
            directions[0] = forward;
            directions[1] = Quaternion.Euler(0, 0, +spreadAngle) * forward;
            directions[2] = Quaternion.Euler(0, 0, -spreadAngle) * forward;

            foreach (var dir in directions)
            {
                StartCoroutine(SpawnFlameWave(dir));
            }
        }

        IEnumerator SpawnFlameWave(Vector3 dir)
        {
            for (int i = 0; i < waveCount; i++)
            {
                Vector3 spawnPos = transform.position + dir * (0.5f + i * 0.3f);
                var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                proj.GetComponent<ExpeditionProjectile>()?.Launch(dir, baseDamage/30);
                yield return new WaitForSeconds(waveSpacing);
            }
        }
    }
}
