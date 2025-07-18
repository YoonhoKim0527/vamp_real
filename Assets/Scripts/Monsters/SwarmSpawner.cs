using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class SwarmSpawner : MonoBehaviour
    {
        private EntityManager entityManager;
        private Character playerCharacter;

        private MonsterBlueprint swarmMonsterBlueprint; // 몬스터 블루프린트
        private GameObject swarmMonsterPrefab;

        private float spawnInterval = 5f; // 몇 초마다 스폰
        private int swarmCount = 20;       // 한 번에 생성될 개수
        private float spawnRadius = 8f;    // 플레이어 기준 생성 반경
        private float swarmMoveSpeed = 6f; // 직진 속도

        private float timer = 0f;

        public void Init(EntityManager manager, Character player, GameObject prefab, MonsterBlueprint blueprint)
        {
            entityManager = manager;
            playerCharacter = player;
            swarmMonsterPrefab = prefab;
            swarmMonsterBlueprint = blueprint;
        }

        public void Tick()
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnSwarm();
            }
        }

        private void SpawnSwarm()
        {
            Debug.Log("[SwarmSpawner] 🦇 박쥐 떼 스폰!");

            Vector2 playerPos = playerCharacter.transform.position;
            Vector2 spawnCenter = playerPos + Random.insideUnitCircle.normalized * spawnRadius;

            for (int i = 0; i < swarmCount; i++)
            {
                Vector2 spawnPos = spawnCenter + Random.insideUnitCircle * 1.5f; // 군집 생성
                Vector2 moveDir = (playerPos - spawnPos).normalized;

                // 풀 대신 직접 Instantiate
                SwarmMonster monster = Instantiate(swarmMonsterPrefab, spawnPos, Quaternion.identity)
                                       .GetComponent<SwarmMonster>();
                monster.Init(entityManager, playerCharacter);
                monster.Setup(-1, spawnPos, swarmMonsterBlueprint, 0f); // 풀 미사용 표시
                monster.InitSwarm(spawnPos, moveDir, swarmMoveSpeed);
            }
        }
    }
}
