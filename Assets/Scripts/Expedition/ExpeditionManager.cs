using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class ExpeditionManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] Transform fighterSpawnParent;
        [SerializeField] Transform characterParent;
        [SerializeField] AbilityManager abilityManager;
        [SerializeField] EntityManager entityManager;
        [SerializeField] StatsManager statsManager; // ✅ 추가됨

        List<Transform> spawnPoints = new();
        List<Character> fighterCharacters = new();
        [SerializeField] Transform bossSpawnPoint;
        public Transform BossSpawnPoint => bossSpawnPoint;

        public void UpdateBossHP(float currentHp, float maxHp)
        {
            Debug.Log($"[Boss HP] {currentHp} / {maxHp}");
        }

        public void OnBossDefeated()
        {
            Debug.Log("[Expedition] Boss defeated!");
        }

        void Awake()
        {
            foreach (Transform child in fighterSpawnParent)
                spawnPoints.Add(child);

            foreach (Transform child in characterParent)
            {
                Character fighter = child.GetComponent<Character>();
                if (fighter != null)
                    fighterCharacters.Add(fighter);
            }
        }

        void Start()
        {
            SetupExpeditionFighters();
        }

        void SetupExpeditionFighters()
        {
            var selectedBlueprints = CrossSceneData.ExpeditionCharacters;

            for (int i = 0; i < fighterCharacters.Count && i < spawnPoints.Count && i < selectedBlueprints.Count; i++)
            {
                Character fighter = fighterCharacters[i];
                Transform spawn = spawnPoints[i];
                CharacterBlueprint blueprint = selectedBlueprints[i];

                fighter.transform.position = spawn.position;

                // ✅ statsManager까지 포함해서 Init 호출
                //fighter.SetBlueprint(blueprint);
                fighter.Init(entityManager, abilityManager, statsManager);
            }
        }
    }
}
