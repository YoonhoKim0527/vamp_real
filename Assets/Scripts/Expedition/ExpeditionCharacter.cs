using UnityEngine;
using System;
using System.Reflection;

namespace Vampire
{
    public class ExpeditionCharacter : MonoBehaviour
    {
        CharacterBlueprint blueprint;
        MonoBehaviour abilityInstance;

        public void Initialize(CharacterBlueprint blueprint, Transform boss)
        {
            this.blueprint = blueprint;

            // ✅ 스킬 클래스 이름 → 타입 얻기
            Type abilityType = Type.GetType(blueprint.abilityClassName);
            if (abilityType == null)
            {
                Debug.LogError($"❌ Ability type '{blueprint.abilityClassName}' not found!");
                return;
            }

            // ✅ 어빌리티 붙이기
            abilityInstance = gameObject.AddComponent(abilityType) as MonoBehaviour;
            if (abilityInstance == null)
            {
                Debug.LogError("❌ Failed to add ability component.");
                return;
            }

            // ✅ projectilePrefab 설정
            var projField = abilityType.GetField("projectilePrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            projField?.SetValue(abilityInstance, blueprint.abilityPrefab);  // 여기 이름 꼭 맞게!

            // ✅ baseDamage 설정
            var setDamage = abilityType.GetMethod("SetDamage", BindingFlags.Public | BindingFlags.Instance);
            setDamage?.Invoke(abilityInstance, new object[] { blueprint.baseDamage });

            // ✅ boss 설정
            var setBoss = abilityType.GetMethod("SetBoss", BindingFlags.Public | BindingFlags.Instance);
            setBoss?.Invoke(abilityInstance, new object[] { boss });

            var setInterval = abilityType.GetMethod("SetFireInterval", BindingFlags.Public | BindingFlags.Instance);
            setInterval?.Invoke(abilityInstance, new object[] { blueprint.expeditionAbilityInterval });

            // ✅ 애니메이션
            var animator = GetComponent<SpriteAnimator>();
            if (animator != null)
            {
                animator.Init(blueprint.walkSpriteSequence, blueprint.walkFrameTime);
                animator.StartAnimating();
            }
        }

        public void SetBoss(Transform newBoss)
        {
            if (abilityInstance == null) return;

            var setBoss = abilityInstance.GetType().GetMethod("SetBoss", BindingFlags.Public | BindingFlags.Instance);
            setBoss?.Invoke(abilityInstance, new object[] { newBoss });
        }
        public string GetBlueprintName()
        {
            return blueprint != null ? blueprint.name : gameObject.name;
        }
    }
}
