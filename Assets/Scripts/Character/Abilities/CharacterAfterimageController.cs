using System.Collections.Generic;
using UnityEngine;
namespace Vampire
{
    public class CharacterAfterimageController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ghostPrefab; // 투명한 캐릭터 sprite 프리팹
        [SerializeField] private float spacing = 0.2f;        // 본체와의 거리 간격
        [SerializeField] private float ghostAlpha = 0.4f;     // 분신 알파값
        [SerializeField] private int maxGhosts = 3;

        private List<Transform> ghostTransforms = new();
        private Character playerCharacter;

        public void Init(Character character, int ghostCount)
        {
            playerCharacter = character;
            maxGhosts = ghostCount;

            for (int i = 0; i < maxGhosts; i++)
            {
                SpriteRenderer ghost = Instantiate(ghostPrefab, transform);
                ghost.color = new Color(1f, 1f, 1f, ghostAlpha); // 반투명
                ghost.sortingOrder = -1; // 본체 뒤
                ghostTransforms.Add(ghost.transform);
            }
        }

        private void LateUpdate()
        {
            if (playerCharacter == null) return;

            for (int i = 0; i < ghostTransforms.Count; i++)
            {
                Vector3 offset = -playerCharacter.transform.right * spacing * (i + 1); // 뒤쪽으로 정렬
                ghostTransforms[i].position = playerCharacter.transform.position + offset;
            }
        }

        public List<Vector3> GetGhostPositions()
        {
            List<Vector3> positions = new();
            foreach (var t in ghostTransforms)
                positions.Add(t.position);
            return positions;
        }
    }
}
