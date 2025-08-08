using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class SplitBossAbility : BossAbility
    {
        [Header("Split Settings")]
        [SerializeField] private GameObject splitBossPrefab; // 분열될 보스 프리팹
        [SerializeField] private float splitThreshold = 0.5f; // 체력 50% 이하일 때 분열
        [SerializeField] private float splitStatRatio = 0.75f; // 75% 능력치로 분열

        private bool hasSplit = false;

        private bool isClone = false;  // ✅ 클론 여부

        public void MarkAsClone()
        {
            isClone = true;
        }


        public override IEnumerator Activate()
        {
            if (hasSplit || monster.HealthPercent > splitThreshold)
                yield break;

            hasSplit = true;

            monster.gameObject.SetActive(false);

            Vector3 basePos = monster.transform.position;
            Vector3 leftPos = basePos + Vector3.left * 1.5f;
            Vector3 rightPos = basePos + Vector3.right * 1.5f;

            BossMonster clone1 = entityManager.SpawnSplitBoss(splitBossPrefab, leftPos, monster.Blueprint, splitStatRatio, monster);
            BossMonster clone2 = entityManager.SpawnSplitBoss(splitBossPrefab, rightPos, monster.Blueprint, splitStatRatio, monster);

            foreach (var ability in clone1.GetComponentsInChildren<SplitBossAbility>())
                ability.MarkAsClone();
            foreach (var ability in clone2.GetComponentsInChildren<SplitBossAbility>())
                ability.MarkAsClone();

            SplitTracker tracker = monster.gameObject.AddComponent<SplitTracker>();
            tracker.Init(new BossMonster[] { clone1, clone2 }, monster);

            yield return null;
        }



        private BossMonster SpawnSplitClone(Vector3 position)
        {
            GameObject cloneGO = Instantiate(splitBossPrefab, position, Quaternion.identity);
            BossMonster clone = cloneGO.GetComponent<BossMonster>();

            int index = entityManager.GetUniqueMonsterIndex(); // 필요 시 유일한 몬스터 인덱스 확보

            // ✅ Setup: 체력, Blueprint, 위치, 애니메이터, 등록 등 모두 초기화
            clone.Setup(index, position, monster.Blueprint);

            // ✅ Init: entityManager, playerCharacter 등록
            clone.Init(entityManager, playerCharacter);

            // ✅ 능력치 조정 (currentHealth, maxHealth, moveSpeed 등)
            clone.CopyStatsFrom(monster, splitStatRatio);

            // ✅ BossAbility들도 Init() 호출
            foreach (var ability in clone.GetComponents<BossAbility>())
            {
                ability.Init(clone, entityManager, playerCharacter);
            }

            return clone;
        }

        public override float Score()
        {
            if (isClone) return 0f;          // ✅ 클론이면 절대 분열 안 함

            if (hasSplit || monster.HealthPercent > splitThreshold)
                return 0f;

            return 1f;
        }
    }
}
