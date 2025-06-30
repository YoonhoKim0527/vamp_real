using UnityEngine;

namespace Vampire
{
    [CreateAssetMenu(fileName = "Skeleton Monster", menuName = "Blueprints/Monsters/Skeleton Monster", order = 1)]
    public class SkeletonMonsterBlueprint : MonsterBlueprint
    {
        [Header("Skeleton Monster")]
        public LayerMask targetLayer;
    }
}