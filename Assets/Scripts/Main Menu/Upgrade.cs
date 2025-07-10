using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class Upgrade : MonoBehaviour
    {
        [SerializeField] private UpgradeItemBlueprint[] upgradeBlueprints;
        [SerializeField] private GameObject upgradeItemCardPrefab;
        [SerializeField] protected CoinDisplay coinDisplay;

        private UpgradeItemCard[] upgradeItemCards;
        public bool IsInitialized { get; private set; } = false;

        public void Init()
        {
            if (IsInitialized)
            {
                Debug.Log("[Upgrade] Already initialized. Skipping Init().");
                return;
            }

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            upgradeItemCards = new UpgradeItemCard[upgradeBlueprints.Length];
            for (int i = 0; i < upgradeBlueprints.Length; i++)
            {
                upgradeItemCards[i] = Instantiate(upgradeItemCardPrefab, transform).GetComponent<UpgradeItemCard>();
                upgradeItemCards[i].Init(this, upgradeBlueprints[i], coinDisplay);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            for (int i = 0; i < upgradeItemCards.Length; i++)
                upgradeItemCards[i].UpdateLayout();

            IsInitialized = true;
        }

        public void RefreshUpgradeUI()
        {
            Debug.Log("[Upgrade] Refreshing Upgrade UI...");
            if (upgradeItemCards == null) return;

            foreach (var card in upgradeItemCards)
            {
                card.Refresh();
            }
        }
    }
}
