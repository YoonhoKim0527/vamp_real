using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class Volcano : MonoBehaviour
    {
        [SerializeField] ObjectPool lavaPool;
        [SerializeField] float interval = 2f;
        [SerializeField] int lavaCount = 3;
        [SerializeField] float lavaRadius = 5f;
        [SerializeField] float damage = 10f;
        [SerializeField] float damageInterval = 1f;

        Dictionary<Character, float> damageTimers = new();

        void OnEnable()
        {
            StartCoroutine(SpawnLoop());
        }

        void OnDisable()
        {
            StopAllCoroutines();
            damageTimers.Clear();
        }

        IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(1f);
            while (true)
            {
                for (int i = 0; i < lavaCount; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * lavaRadius;
                    Vector3 spawnPos = transform.position + (Vector3)offset;

                    GameObject lava = lavaPool.Get();
                    lava.transform.position = spawnPos;
                    lava.SetActive(true);
                }
                yield return new WaitForSeconds(interval);
            }
        }

        void OnCollisionStay2D(Collision2D other)
        {
            if (other.collider.CompareTag("Player"))
            {
                Character character = other.collider.GetComponent<Character>();
                if (character == null) return;

                if (!damageTimers.ContainsKey(character))
                    damageTimers[character] = 0f;

                damageTimers[character] += Time.deltaTime;

                if (damageTimers[character] >= damageInterval)
                {
                    character.TakeDamage(damage);
                    damageTimers[character] = 0f;
                }
            }
        }

        void OnCollisionExit2D(Collision2D other)
        {
            if (other.collider.CompareTag("Player"))
            {
                Character character = other.collider.GetComponent<Character>();
                if (character != null && damageTimers.ContainsKey(character))
                {
                    damageTimers.Remove(character);
                }
            }
        }
    }
}
