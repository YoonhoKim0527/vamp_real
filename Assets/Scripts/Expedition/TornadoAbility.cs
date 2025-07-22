using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class TornadoAbility : BaseExpeditionAbility
    {
        [SerializeField] GameObject projectilePrefab;

        protected override void TriggerAbility()
        {
            var tornado = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            tornado.GetComponent<TornadoProjectile>()?.Launch(boss, baseDamage);
        }
    }
}

