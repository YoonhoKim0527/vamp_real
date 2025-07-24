using TMPro;
using UnityEngine;

namespace Vampire
{
    public class StatsUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI statValueText;

        public void SetStat(string name, string value)
        {
            statNameText.text = name;
            statValueText.text = value;
        }
    }
}