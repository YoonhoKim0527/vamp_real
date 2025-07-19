using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    [System.Serializable]
    public class FarmSlot
    {
        public CharacterBlueprint bp;
        public float lastCollectedTime;        // 마지막 정산 시각
    }

}
