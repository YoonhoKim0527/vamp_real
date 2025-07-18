using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class FlowerSpawner : MonoBehaviour
    {
        private EntityManager entityManager;
        private Character playerCharacter;

        private MonsterBlueprint flowerMonsterBlueprint;

        private float spawnInterval = 10f; // 몇 초마다 스폰
        private int flowerCount = 30;      // 원형으로 생성될 개수
        private float spawnRadius = 5f;    // 플레이어 기준 생성 반경
        private GameObject flowerMonsterPrefab;

        private float timer = 0f;

        public void Init(EntityManager manager, Character player, GameObject prefab, MonsterBlueprint blueprint)
        {
            entityManager = manager;
            playerCharacter = player;
            flowerMonsterPrefab = prefab;
            flowerMonsterBlueprint = blueprint;
        }

        public void Tick()
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnFlowers();
            }
        }

        private void SpawnFlowers()
        {
            Debug.Log("[FlowerSpawner] 🌸 꽃 몬스터 생성!");

            Vector2 playerPos = playerCharacter.transform.position;

            for (int i = 0; i < flowerCount; i++)
            {
                float angle = i * Mathf.PI * 2 / flowerCount; // 원형 배치
                Vector2 spawnPos = playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;

                // 몬스터 직접 생성 (풀 사용 안함)
                FlowerMonster monster = Instantiate(flowerMonsterPrefab, spawnPos, Quaternion.identity)
                                        .GetComponent<FlowerMonster>();
                monster.Init(entityManager, playerCharacter);
                monster.Setup(-1, spawnPos, flowerMonsterBlueprint, 0f); // -1: 풀 미사용 표시
            }
        }
    }
}
