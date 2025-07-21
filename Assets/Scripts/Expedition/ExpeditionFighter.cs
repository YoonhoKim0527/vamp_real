using UnityEngine;

namespace Vampire
{
    public class ExpeditionFighter : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] SpriteAnimator spriteAnimator;

        CharacterBlueprint blueprint;
        ExpeditionBoss boss;

        public void Init(CharacterBlueprint blueprint, ExpeditionBoss boss)
        {
            this.blueprint = blueprint;
            this.boss = boss;

            // 초기 정지 이미지
            spriteRenderer.sprite = blueprint.walkSpriteSequence?[0];

            // 🔥 걷기 애니메이션 시작
            if (spriteAnimator != null && blueprint.walkSpriteSequence != null && blueprint.walkSpriteSequence.Length > 0)
            {
                spriteAnimator.Init(blueprint.walkSpriteSequence, blueprint.walkFrameTime, true);
                spriteAnimator.StartAnimating(true);
            }

            SpawnStartingAbility();
        }
        void SpawnStartingAbility()
        {
            if (blueprint.startingAbilities == null || blueprint.startingAbilities.Length == 0)
                return;

            var prefab = blueprint.startingAbilities[0];
            if (prefab == null) return;

            var abilityObj = Instantiate(prefab, transform.position, Quaternion.identity, transform);

            if (abilityObj.TryGetComponent<IExpeditionAbility>(out var expeditionAbility))
            {
                expeditionAbility.InitExpeditionAbility(transform, boss.transform, blueprint.baseDamage);
            }
            else
            {
                Debug.LogWarning($"{blueprint.name}의 startingAbility는 IExpeditionAbility를 구현하고 있지 않습니다.");
            }
        }
    }
}