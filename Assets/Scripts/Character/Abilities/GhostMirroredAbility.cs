using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Vampire
{
    public class GhostMirroredAbility : Ability
    {
        [Header("Ghost 설정")]
        [SerializeField] private int ghostCount = 5;
        [SerializeField] private int frameSpacing = 15; // 각 고스트 간 시간차 (프레임 단위)
        [SerializeField] private float skillCooldown = 1f;

        private AbilityManager abilityManager;
        private Character playerCharacter;
        private SpriteRenderer playerSpriteRenderer;

        private List<GameObject> ghostList = new();
        private List<SpriteRenderer> ghostRenderers = new();
        private List<int> ghostDelays = new();
        private Queue<Vector3> positionBuffer = new();  // 전체 기록

        private float timer = 0f;
        private bool initialized = false;

        public void Init(AbilityManager manager)
        {
            abilityManager = manager;
            playerCharacter = abilityManager.PlayerCharacter;

            if (playerCharacter == null)
            {
                Debug.LogError("[GhostMirroredAbility] PlayerCharacter not found.");
                return;
            }

            Transform spriteTransform = playerCharacter.transform.Find("Character Sprite");
            if (spriteTransform != null)
            {
                playerSpriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            }

            if (playerSpriteRenderer == null)
            {
                Debug.LogError("[GhostMirroredAbility] SpriteRenderer not found.");
                return;
            }

            SpawnGhosts();
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;

            // 현재 위치 기록
            positionBuffer.Enqueue(playerCharacter.transform.position);
            if (positionBuffer.Count > ghostCount * frameSpacing + 1)
                positionBuffer.Dequeue();

            // 고스트 위치 반영
            for (int i = 0; i < ghostList.Count; i++)
            {
                int index = Mathf.Max(0, positionBuffer.Count - 1 - ghostDelays[i]);
                Vector3[] posArray = positionBuffer.ToArray();
                ghostList[i].transform.position = posArray[index];
            }

            // 쿨다운 처리
            timer += Time.deltaTime;
            if (timer >= skillCooldown)
            {
                timer = 0f;
                TriggerGhostSkills();
            }
        }

        private void SpawnGhosts()
        {
            for (int i = 0; i < ghostCount; i++)
            {
                GameObject ghost = new GameObject($"GhostFollower_{i}");
                SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();

                sr.sprite = playerSpriteRenderer.sprite;
                sr.color = new Color(1f, 1f, 1f, 1f); // 투명도
                sr.sortingLayerID = playerSpriteRenderer.sortingLayerID;
                sr.sortingOrder = playerSpriteRenderer.sortingOrder - 2 - i; // 확실히 뒤에

                ghost.transform.localScale = playerCharacter.transform.localScale;
                ghost.transform.position = playerCharacter.transform.position;

                ghostList.Add(ghost);
                ghostRenderers.Add(sr);
                ghostDelays.Add((i + 1) * frameSpacing); // 더 오래된 위치 사용
            }
        }

        private void TriggerGhostSkills()
        {
            var owned = abilityManager.GetOwnedAbilities()
                .Where(a => a.Owned && a.Level > 0)
                .ToList();

            if (owned.Count == 0) return;

            Ability selected = owned[Random.Range(0, owned.Count)];

            for (int i = 0; i < ghostList.Count; i++)
            {
                Vector3 ghostPos = ghostList[i].transform.position;
                selected.MirrorActivate(0.5f, ghostPos, ghostColor: GetGhostColor(i));
            }
        }

        private Color GetGhostColor(int index)
        {
            // 예시: 분신마다 다른 색상 주기 (파랑~보라)
            float hue = 0.6f + 0.1f * index;
            return Color.HSVToRGB(hue % 1f, 0.6f, 1f);
        }
    }
}
