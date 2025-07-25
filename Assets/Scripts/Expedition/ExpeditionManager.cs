using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Vampire
{
    public class ExpeditionManager : MonoBehaviour
    {
        [Header("Characters")]
        [SerializeField] CharacterBlueprint[] allBlueprints;
        [SerializeField] CharacterBlueprint[] selectedCharacters;
        [SerializeField] Transform[] characterSpawnPoints;

        [Header("Boss Settings")]
        [SerializeField] ExpeditionBossBlueprint[] bossBlueprints;
        [SerializeField] GameObject bossPrefab;
        [SerializeField] Transform bossSpawnPoint;

        ExpeditionBoss currentBoss;
        int currentBossIndex = 0;

        List<ExpeditionCharacter> spawnedCharacters = new List<ExpeditionCharacter>();
        [SerializeField] ExpeditionEntityManager entityManager;
        [SerializeField] SaveManager saveManager;

        void OnApplicationQuit()
        {
            Debug.Log("🛑 어플리케이션 종료 감지 → 저장 실행");
            SaveExpeditionData();
        }
        void Start()
        {
            LoadExpeditionData(); // ✅ Expedition 씬에 진입하면 바로 호출됨
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

        void SpawnBoss(bool isLoading = false)
        {
            if (currentBossIndex >= bossBlueprints.Length)
            {
                Debug.Log("🎉 모든 보스를 처치했습니다! 게임 클리어!");
                return;
            }

            var blueprint = bossBlueprints[currentBossIndex];
            var bossGO = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            currentBoss = bossGO.GetComponent<ExpeditionBoss>();
            currentBoss.Initialize(blueprint, entityManager);
            currentBoss.OnDeath += HandleBossDeath;

            var tapDamage = FindObjectOfType<UITapDamage>();
            if (tapDamage != null)
            {
                tapDamage.SetBoss(currentBoss);
            }

            // ✅ 여기서는 증가시키지 않는다.
        }
        void HandleBossDeath()
        {
            Debug.Log("☠️ 보스 사망 → 다음 보스 생성");

            // 기존 발사체 제거
            foreach (var proj in GameObject.FindObjectsOfType<ExpeditionProjectile>())
                Destroy(proj.gameObject);

            currentBossIndex++; // ✅ 여기서만 증가

            SpawnBoss(); // 다음 보스 소환

            if (currentBoss != null)
            {
                foreach (var character in spawnedCharacters)
                    character.SetBoss(currentBoss.transform);
            }
        }
        public bool TrySelectCharacter(CharacterBlueprint blueprint)
        {
            for (int i = 0; i < selectedCharacters.Length; i++)
            {
                if (selectedCharacters[i] == null)
                {
                    selectedCharacters[i] = blueprint;
                    SpawnCharacterAt(i, blueprint);
                    return true;
                }
            }
            Debug.Log("❗ 최대 6마리까지만 선택할 수 있습니다.");
            return false;
        }

        public void RemoveSelectedCharacter(CharacterBlueprint blueprint)
        {
            for (int i = 0; i < selectedCharacters.Length; i++)
            {
                if (selectedCharacters[i] == blueprint)
                {
                    selectedCharacters[i] = null;

                    // ✅ name이 아니라 blueprint 기준으로 삭제
                    var toRemove = spawnedCharacters.FirstOrDefault(c => c != null && c.GetBlueprintName() == blueprint.name);
                    if (toRemove != null)
                    {
                        spawnedCharacters.Remove(toRemove);
                        Destroy(toRemove.gameObject);
                    }

                    return;
                }
            }
        }


        public bool IsCharacterSelected(CharacterBlueprint blueprint)
        {
            foreach (var b in selectedCharacters)
            {
                if (b == blueprint) return true;
            }
            return false;
        }
        void SpawnCharacterAt(int index, CharacterBlueprint blueprint)
        {
            if (index >= characterSpawnPoints.Length) return;

            Transform spawnPoint = characterSpawnPoints[index];

            GameObject go = new GameObject($"ExpeditionChar_{index}_{blueprint.name}");
            go.transform.position = spawnPoint.position;

            if (index % 2 == 1)
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
            character.Initialize(blueprint, currentBoss?.transform);
            spawnedCharacters.Add(character);

        }
void LoadExpeditionData()
{
    var data = saveManager.LoadExpeditionData();
    Debug.Log("🚀 불러오기 시도");

    currentBossIndex = data.bossIndex;

    SpawnBoss(isLoading: true);
    currentBoss?.SetHP(data.bossCurrentHP);

    // 캐릭터 슬롯 초기화
    for (int i = 0; i < selectedCharacters.Length; i++)
        selectedCharacters[i] = null;

    // 캐릭터 오브젝트 제거
    foreach (var existingChar in spawnedCharacters)
        Destroy(existingChar.gameObject);
    spawnedCharacters.Clear();

    // 캐릭터 복원 (씬 오브젝트 + 슬롯 모두)
    foreach (var c in data.characters)
    {
        var blueprint = FindBlueprintByName(c.characterName);
        if (blueprint == null) continue;

        // 슬롯에 이미 있는지 확인 (중복 방지)
        int slotIndex = -1;
        if (data.selectedCharacterNames != null)
        {
            slotIndex = data.selectedCharacterNames.FindIndex(name => name == c.characterName);
            if (slotIndex >= 0 && slotIndex < selectedCharacters.Length)
                selectedCharacters[slotIndex] = blueprint;
        }

        // 오브젝트 생성
        GameObject go = new GameObject($"ExpeditionChar_{c.characterName}");
        go.transform.position = c.position;

        if (c.facingLeft)
            go.transform.localScale = new Vector3(-1, 1, 1);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = blueprint.walkSpriteSequence.Length > 0 ? blueprint.walkSpriteSequence[0] : null;

        var animator = go.AddComponent<SpriteAnimator>();
        animator.Init(blueprint.walkSpriteSequence, blueprint.walkFrameTime);
        animator.StartAnimating();

        var character = go.AddComponent<ExpeditionCharacter>();
        character.Initialize(blueprint, currentBoss?.transform);
        spawnedCharacters.Add(character);
    }

    // 부스트 복원
    foreach (var boost in data.activeBoosts)
        BoostManager.Instance.ActivateBoost(boost.type, 2f, boost.remainingTime);
}

        CharacterBlueprint FindBlueprintByName(string name)
        {
            return System.Array.Find(allBlueprints, bp => bp.name == name);
        }
void SaveExpeditionData()
{
    Debug.Log($"[Save] 보스: {currentBoss?.name}, HP: {currentBoss?.HP}");
    Debug.Log($"[Save] 캐릭터 수: {spawnedCharacters.Count}");

    saveManager.SaveExpeditionData(
        currentBossIndex,
        currentBoss?.HP ?? 0f,
        currentBoss?.name,
        spawnedCharacters.ConvertAll(go => go.gameObject),
        new List<CharacterBlueprint>(selectedCharacters)
    );
}

    }
    
    
}
