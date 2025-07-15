using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class MassKillAbility : Ability
    {
        [Header("Mass Kill Settings")]
        [SerializeField] private Sprite killZoneSprite;
        [SerializeField] private float zoneDisplayTime = 1f;
        [SerializeField] protected UpgradeableWeaponCooldown cooldown;
        [SerializeField] private LayerMask monsterLayer;

        protected override void Use()
        {
            base.Use();
            Debug.Log("MassKillAbility Use()");
            gameObject.SetActive(true); // 🔥 Ability가 비활성화되는걸 막음
            StartCoroutine(MassKillLoop());
        }

        private IEnumerator MassKillLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(cooldown.Value);
                Debug.Log("hi22");
                StartCoroutine(ActivateMassKill());
            }
        }

        private IEnumerator ActivateMassKill()
        {
            Debug.Log("[MassKill] 궁극기 발동!");

            // 화면 중앙 이미지 생성
            GameObject zone = new GameObject("MassKillZone");
            SpriteRenderer sr = zone.AddComponent<SpriteRenderer>();
            sr.sprite = killZoneSprite;
            sr.sortingOrder = 1000;
            zone.transform.position = playerCharacter.CenterTransform.position;
            zone.transform.localScale = Vector3.one * 5f;

            yield return new WaitForSeconds(zoneDisplayTime);

            // 영역 내 몬스터 즉사
            Vector2 center = zone.transform.position;
            float radius = sr.bounds.extents.x;

            Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(center, radius, monsterLayer);
            Debug.Log($"[MassKill] 감지된 몬스터 수: {hitMonsters.Length}");

            foreach (Collider2D collider in hitMonsters)
            {
                Monster monster = collider.GetComponent<Monster>();
                if (monster != null)
                {
                    Debug.Log($"[MassKill] {monster.name} 즉사 처리");
                    monster.TakeDamage(float.MaxValue, Vector2.zero);
                }
            }

            Destroy(zone);
        }
    }
}
