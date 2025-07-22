using UnityEngine;
using System.Collections.Generic;

namespace Vampire
{
    public class ExpeditionManager : MonoBehaviour
    {
        [Header("Characters")]
        [SerializeField] CharacterBlueprint[] selectedCharacters;
        [SerializeField] Transform[] characterSpawnPoints;

        [Header("Boss Settings")]
        [SerializeField] ExpeditionBossBlueprint[] bossBlueprints;
        [SerializeField] GameObject bossPrefab;
        [SerializeField] Transform bossSpawnPoint;

        ExpeditionBoss currentBoss;
        int currentBossIndex = 0;

        List<ExpeditionCharacter> spawnedCharacters = new List<ExpeditionCharacter>();

        void Start()
        {
            SpawnBoss();
            SpawnCharacters(currentBoss.transform);
        }
 
        void SpawnCharacters(Transform bossTransform)
        {
            for (int i = 0; i < selectedCharacters.Length && i < characterSpawnPoints.Length; i++)
            {
                var blueprint = selectedCharacters[i];
                if (blueprint == null) continue;

                GameObject go = new GameObject($"ExpeditionChar_{i}_{blueprint.name}");
                go.transform.position = characterSpawnPoints[i].position;

                if (i % 2 == 1)
                {
                    var scale = go.transform.localScale;
                    scale.x = -1;
                    go.transform.localScale = scale;
                }

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = blueprint.walkSpriteSequence.Length > 0 ? blueprint.walkSpriteSequence[0] : null;

                var animator = go.AddComponent<SpriteAnimator>();
                animator.Init(blueprint.walkSpriteSequence, blueprint.walkFrameTime);
                animator.StartAnimating();

                var character = go.AddComponent<ExpeditionCharacter>();
                character.Initialize(blueprint, bossTransform);
                spawnedCharacters.Add(character); // ✅ 저장
            }
        }

        void SpawnBoss()
        {
            if (currentBossIndex >= bossBlueprints.Length)
            {
                Debug.Log("🎉 모든 보스를 처치했습니다! 게임 클리어!");
                return;
            }

            var blueprint = bossBlueprints[currentBossIndex];
            var bossGO = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            currentBoss = bossGO.GetComponent<ExpeditionBoss>();
            currentBoss.Initialize(blueprint);
            currentBoss.OnDeath += HandleBossDeath;

            currentBossIndex++;
        }

        void HandleBossDeath()
        {
            Debug.Log("☠️ 보스 사망 → 다음 보스 생성");
            
            // 🔥 현재 있는 모든 ExpeditionProjectile 제거
            foreach (var proj in GameObject.FindObjectsOfType<ExpeditionProjectile>())
            {
                Destroy(proj.gameObject);
            }

            SpawnBoss();

            if (currentBoss != null)
            {
                foreach (var character in spawnedCharacters)
                {
                    character.SetBoss(currentBoss.transform);
                }
            }
        }
    }
}
