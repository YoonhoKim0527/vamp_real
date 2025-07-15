using UnityEngine;

namespace Vampire
{
    public class Lava : MonoBehaviour
    {
        [SerializeField] float duration = 2f;
        [SerializeField] float damage = 10f;

        void OnEnable()
        {
            CancelInvoke();
            Invoke(nameof(Deactivate), duration);
        }

        void Deactivate()
        {
            gameObject.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Character character = other.GetComponent<Character>();
                if (character != null)
                    character.TakeDamage(damage);
            }
        }
    }
}
