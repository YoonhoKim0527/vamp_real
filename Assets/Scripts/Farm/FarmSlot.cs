using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    [System.Serializable]
    public class FarmSlot
    {
        public CharacterBlueprint character;           // 배치된 캐릭터
        public float lastCollectedTime;               // 마지막 수확 시각
        public float maxAccumSec = 3600f;             // 최대 1시간 누적

        public float PendingReward
        {
            get
            {
                if (character == null) return 0;
                float elapsed = Mathf.Min(Time.realtimeSinceStartup - lastCollectedTime, maxAccumSec);
                return elapsed * character.farmProductionPerSecond;
            }
        }

        public void Collect()
        {
            lastCollectedTime = Time.realtimeSinceStartup;
        }
    }

}
