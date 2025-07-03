using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class Upgrade : MonoBehaviour
    {
        [SerializeField] private UpgradeItemBlueprint[] upgradeBlueprints;
        [SerializeField] private GameObject upgradeItemCardPrefab;
        [SerializeField] private CoinDisplay coinDisplay;

        private UpgradeItemCard[] upgradeItemCards;

        public void Init()
        {
            upgradeItemCards = new UpgradeItemCard[upgradeBlueprints.Length];
            for (int i = 0; i < upgradeBlueprints.Length; i++)
            {
                upgradeItemCards[i] = Instantiate(upgradeItemCardPrefab, transform).GetComponent<UpgradeItemCard>();
                upgradeItemCards[i].Init(this, upgradeBlueprints[i], coinDisplay);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            for (int i = 0; i < upgradeItemCards.Length; i++)
                upgradeItemCards[i].UpdateLayout();
            
            Debug.Log("ssssssssssssssssssssssssssssssss" + upgradeBlueprints.Length);
        }
    }
}
