using UnityEngine;
using System.Collections.Generic;

namespace Vampire
{
    public class ExpeditionEntityManager : MonoBehaviour
    {
        [SerializeField] GameObject damageTextPrefab;
        Queue<ExpeditionDamageText> pool = new();

        public ExpeditionDamageText SpawnDamageText(Vector2 position, float damage, bool isCritical = false)
        {
            ExpeditionDamageText text;
            if (pool.Count > 0)
            {
                text = pool.Dequeue();
                text.gameObject.SetActive(true);
            }
            else
            {
                var go = Instantiate(damageTextPrefab);
                text = go.GetComponent<ExpeditionDamageText>();
                text.Init(this);
            }

            text.Setup(position, damage, isCritical);
            return text;
        }

        public void DespawnDamageText(ExpeditionDamageText text)
        {
            text.gameObject.SetActive(false);
            pool.Enqueue(text);
        }
    }
}
