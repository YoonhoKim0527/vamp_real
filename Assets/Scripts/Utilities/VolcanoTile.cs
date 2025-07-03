using UnityEngine;

namespace Vampire
{
    public class VolcanoTile : MonoBehaviour
    {
        [SerializeField] private float damagePerSecond = 10f;

        private bool playerInZone = false;
        private Character playerCharacter; // 플레이어의 Character 컴포넌트

        private void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log(collider);
            if (collider.CompareTag("Player"))
            {
                playerInZone = true;

                // Character 또는 PlayerCharacter 스크립트를 가져온다
                playerCharacter = collider.GetComponentInParent<Character>();
                if (playerCharacter == null)
                {
                    Debug.LogWarning("🔥 VolcanoTile: Character component not found on Player!");
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.CompareTag("Player"))
            {
                playerInZone = false;
                playerCharacter = null;
            }
        }

        private void Update()
        {
            if (playerInZone && playerCharacter != null)
            {
                Debug.Log("hi");
                playerCharacter.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}
