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

            Type abilityType = Type.GetType(blueprint.abilityClassName);
            if (abilityType == null)
            {
                Debug.LogError($"❌ Ability type '{blueprint.abilityClassName}' not found!");
                return;
            }

            abilityInstance = gameObject.AddComponent(abilityType) as MonoBehaviour;
            if (abilityInstance == null)
            {
                Debug.LogError("❌ Failed to add ability component.");
                return;
            }

            var saveManager = FindObjectOfType<SaveManager>();
            var upgradeData = saveManager.GetExpeditionUpgradeData();


            // ✅ projectilePrefab 설정
            var projField = abilityType.GetField("projectilePrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            projField?.SetValue(abilityInstance, blueprint.abilityPrefab);

            // ✅ boss 설정
            var setBoss = abilityType.GetMethod("SetBoss", BindingFlags.Public | BindingFlags.Instance);
            setBoss?.Invoke(abilityInstance, new object[] { boss });


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
