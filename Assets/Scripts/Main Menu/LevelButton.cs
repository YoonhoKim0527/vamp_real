using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] DialogBox characterSelectDialog;
        [SerializeField] CharacterSelector characterSelector;
        [SerializeField] DialogBox currentDialog;
        [SerializeField] private LevelBlueprint levelBlueprint;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                CrossSceneData.LevelBlueprint = levelBlueprint;

                currentDialog?.Close(); // 현재 맵 선택 창 닫기
                characterSelectDialog.Open(); // 캐릭터 선택 창 열기
                characterSelector.Init();
            });
        }
    }
}
