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

        public void Init()
        {
            shopItemCards = new ShopItemCard[itemBlueprints.Length];
            Debug.Log(itemBlueprints.Length);
            for (int i = 0; i < itemBlueprints.Length; i++)
            {
                shopItemCards[i] = Instantiate(shopItemCardPrefab, this.transform).GetComponent<ShopItemCard>();
                Debug.Log(shopItemCards[i] == null);
                shopItemCards[i].Init(this, itemBlueprints[i], coinDisplay);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            for (int i = 0; i < itemBlueprints.Length; i++)
            {
                shopItemCards[i].UpdateLayout();
            }
        }
    }
}