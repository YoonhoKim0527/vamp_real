using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class FixedDirectionStabAbility : StabAbility
    {
        /// <summary>
        /// 일반 공격 시 고정 방향(왼쪽/오른쪽)으로 찌르기
        /// </summary>
        protected override void Attack()
        {
            Vector2 dir = playerCharacter.LookDirection.x > 0 ? Vector2.right : Vector2.left;
            StartCoroutine(Stab(playerCharacter.CenterTransform.position, dir, false));
        }

        /// <summary>
        /// 고스트 복제 시 오른쪽 기준 방향으로 찌르기 (필요 시 외부에서 방향 전달 가능)
        /// </summary>
        public override void MirrorActivate(Vector2 spawnPosition, Vector2 _ignored)
        {
            Vector2 dir = Vector2.right; // 또는 필요 시 Vector2.left 또는 방향 추론
            StartCoroutine(Stab(spawnPosition, dir, true));
        }
    }
}
