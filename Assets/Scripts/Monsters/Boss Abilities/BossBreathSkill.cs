using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class BossBreathSkill : BossAbility
    {
        [Header("Breath Settings")]
        [SerializeField] private GameObject breathAttackPrefab;
        [SerializeField] private float skillDuration = 3f;
        [SerializeField] private float cooldown = 10f;

        [Header("Sprite Control")]
        [SerializeField] private Sprite breathSprite;

        private float nextAvailableTime = 0f;

        public override IEnumerator Activate()
        {
            if (Time.time < nextAvailableTime)
                yield break;

            nextAvailableTime = Time.time + cooldown;
            active = true;

            // 보스 정지
            monster.Stop();

            // ✅ x축 방향만 선택
            Vector2 rawDir = monster.LookDirection;
            Vector2 dir = rawDir.x >= 0 ? Vector2.right : Vector2.left;

            // BreathAttack 프리팹 Instantiate
            GameObject go = Instantiate(breathAttackPrefab, monster.transform.position, Quaternion.identity);
            BreathAttack breath = go.GetComponent<BreathAttack>();
            breath.Init(dir, skillDuration, monster.SpriteRenderer, breathSprite);

            yield return new WaitForSeconds(skillDuration);
            active = false;
        }

        public override float Score()
        {
            return Time.time >= nextAvailableTime ? 1f : 0f;
        }
    }
}
