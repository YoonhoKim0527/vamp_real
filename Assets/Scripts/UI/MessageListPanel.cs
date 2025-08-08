using UnityEngine;

namespace Vampire
{
    public class MessageListPanel : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] GameObject messageItemPrefab;

        void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);

            foreach (var msg in MessageManager.Instance.Messages)
            {
                var go = Instantiate(messageItemPrefab, container);
                var ui = go.GetComponent<MessageUI>();
                ui.Init(msg);
            }
        }
    }
}
