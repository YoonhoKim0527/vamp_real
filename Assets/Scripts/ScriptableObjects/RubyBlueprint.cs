using UnityEngine;

namespace Vampire
{
    [CreateAssetMenu(fileName = "Ruby", menuName = "Blueprints/Ruby", order = 1)]
    public class RubyBlueprint : ScriptableObject
    {
        public EnumDataContainer<CoinType, Sprite> rubySprites;
    }
}
