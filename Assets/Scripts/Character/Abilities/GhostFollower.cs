using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Vampire
{
    public class GhostFollower : MonoBehaviour
    {
        public float followDistance = 1.0f;
        public float followSpeed = 5f;
        public float mirrorCooldown = 2f;

        private Character playerCharacter;
        private AbilityManager abilityManager;
        private float timer = 0f;

        private SpriteRenderer spriteRenderer;

        public void Init(Character player, AbilityManager manager, Sprite sprite, int sortingLayerID, int sortingOrder)
        {
            playerCharacter = player;
            abilityManager = manager;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
            spriteRenderer.sortingLayerID = sortingLayerID;
            spriteRenderer.sortingOrder = sortingOrder - 1;
        }

        void Update()
        {
            if (playerCharacter == null) return;

            // 🧭 따라가기
            Vector3 target = playerCharacter.transform.position - playerCharacter.transform.right * followDistance;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * followSpeed);

            // ⏱️ 쿨다운 체크
            timer += Time.deltaTime;
            if (timer >= mirrorCooldown)
            {
                timer = 0f;
                ActivateMirrorAbility();
            }
        }

        private void ActivateMirrorAbility()
        {
            var abilities = abilityManager.GetOwnedAbilities()
                .Where(a => a.Owned && a.Level > 0)
                .ToList();

            if (abilities.Count == 0) return;

            Ability selected = abilities[Random.Range(0, abilities.Count)];

            Color ghostColor = new Color(0.5f, 0.7f, 1f, 0.6f); // 👻 유령용 스킬 색상
            selected.MirrorActivate(0.5f, transform.position, ghostColor);
        }

    }
}