using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vampire
{
    public class Shop : MonoBehaviour
    {
        [SerializeField] private ShopItemBlueprint[] itemBlueprints;
        [SerializeField] private GameObject shopItemCardPrefab;
        [SerializeField] protected CoinDisplay coinDisplay;

        private ShopItemCard[] shopItemCards;

        // ✅ 중복 초기화 방지 플래그
        public bool IsInitialized { get; private set; } = false;

        public void Init()
        {
            if (IsInitialized)
            {
                Debug.Log("[Shop] Already initialized. Skipping Init().");
                return;
            }

            // ✅ 기존 카드가 있으면 전부 제거 (중복 방지)
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            shopItemCards = new ShopItemCard[itemBlueprints.Length];
            Debug.Log($"[Shop] Initializing {itemBlueprints.Length} item cards.");

            for (int i = 0; i < itemBlueprints.Length; i++)
            {
                shopItemCards[i] = Instantiate(shopItemCardPrefab, this.transform).GetComponent<ShopItemCard>();
                shopItemCards[i].Init(this, itemBlueprints[i], coinDisplay);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            for (int i = 0; i < itemBlueprints.Length; i++)
            {
                shopItemCards[i].UpdateLayout();
            }

            // ✅ 이제 초기화 완료 상태로 표시
            IsInitialized = true;
        }

        public void RefreshShopUI()
        {
            Debug.Log("[Shop] Refreshing Shop UI...");
            if (shopItemCards == null) return;

            foreach (var card in shopItemCards)
            {
                card.Refresh();
            }
        }
    }
}
