using UnityEngine;

namespace Vampire
{
    [CreateAssetMenu(menuName = "Blueprints/Monsters/Expedition Boss Blueprint", fileName = "New Expedition Boss")]
    public class ExpeditionBossBlueprint : ScriptableObject
    {
        [Header("스테이지 정보")]
        public string stageName;
        public Sprite bossPortrait;

        [Header("전투 정보")]
        public float hp;
        public Sprite[] breatheAnimation;
        public float frameTime;

        [Header("보상")]
        public float rewardGold;
        public float rewardExp;
        public int rewardEmerald;
        public GameObject rewardChestPrefab;
    }
}
