using UnityEngine;

namespace Vampire
{
    public class FarmManager : MonoBehaviour
    {
        [SerializeField] CharacterBlueprint[] allBlueprints;
        [SerializeField] GameObject farmCharacterPrefab; // ���� ������ (SpriteRenderer + Animator)
        [SerializeField] Transform farmArea;

        void Start()
        {
            foreach (var blueprint in allBlueprints)
            {
                if (!blueprint.owned) continue;

                var obj = Instantiate(farmCharacterPrefab, GetRandomPosition(), Quaternion.identity, farmArea);
                var controller = obj.GetComponent<FarmCharacterMovement>();
                controller.Init(blueprint);
            }
        }

        Vector3 GetRandomPosition()
        {
            float x = Random.Range(-5f, 5f);
            float y = Random.Range(-3f, 3f);
            return new Vector3(x, y, 0f);
        }
    }
}
