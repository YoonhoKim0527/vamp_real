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

                currentDialog?.Close(); // ���� �� ���� â �ݱ�
                characterSelectDialog.Open(); // ĳ���� ���� â ����
                characterSelector.Init();
            });
        }
    }
}
