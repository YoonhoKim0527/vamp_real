using UnityEngine;

namespace Vampire
{
    public class UITapDamage : MonoBehaviour
    {
        float damageAmount = 10f;
        ExpeditionBoss boss;

        public void SetBoss(ExpeditionBoss boss)
        {
            this.boss = boss;
            Debug.Log($"🧭 UITapDamage: Boss 연결됨 → {boss.gameObject.name}");
        }

        public void OnTapButtonPressed()
        {
            if (boss != null)
            {
                Debug.Log("👆 Tap 버튼 클릭됨 → 보스에게 데미지 부여 시도");
                boss.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("🚨 UITapDamage: 보스가 연결되지 않음 (null)");
            }
        }
    }
}
