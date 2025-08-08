using UnityEngine;

namespace Vampire
{
    [CreateAssetMenu(fileName = "Ruby", menuName = "Blueprints/Ruby", order = 1)]
    public class RubyBlueprint : ScriptableObject
    {
        // 아직 쓸일 없을듯
        public EnumDataContainer<CoinType, Sprite> rubySprites;
    }
}
