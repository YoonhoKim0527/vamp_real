using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public interface IExpeditionAbility
    {
        void InitExpeditionAbility(Transform attacker, Transform bossTarget, float baseDamage);
    }
}


