using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class PoisonCloud : MonoBehaviour
    {
        float damagePerTick;
        float radius;
        float duration;
        float tickInterval = 0.5f;

        LayerMask monsterLayer;
        Character owner;
        protected CharacterStatBlueprint playerStats; // ✅ 스탯 블루프린트로 교체

        public void Init(float damage, float radius, float duration, LayerMask layer, Character owner, CharacterStatBlueprint playerStats)
        {
            this.damagePerTick = damage;
            this.radius = radius;
            this.duration = duration;
            this.monsterLayer = layer;
            this.owner = owner;
            this.playerStats = playerStats; // ✅ 플레이어 스탯 주입
            transform.localScale = Vector3.one * radius * 0.2f;
            StartCoroutine(ApplyEffect());
        }

        IEnumerator ApplyEffect()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, monsterLayer);
                foreach (var hit in hits)
                {
                    IDamageable d = hit.GetComponentInParent<IDamageable>();
                    if (d != null)
                    {
                        // ✅ 플레이어 스탯 기반 데미지 계산
                        float totalDamage = playerStats.attackPower * damagePerTick;

                        // ✅ 치명타 적용
                        if (Random.value < playerStats.criticalChance)
                        {
                            totalDamage *= (1 + playerStats.criticalDamage);
                            Debug.Log("☠️ [PoisonCloud] Critical hit!");
                        }

                        d.TakeDamage(totalDamage, Vector2.zero);
                        owner.OnDealDamage.Invoke(totalDamage);
                    }
                }

                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            Destroy(gameObject); // or gameObject.SetActive(false) if using pooling
        }
    }
}
