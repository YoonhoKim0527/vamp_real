using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    [System.Serializable]
    public class MessageData
    {
        public string id;
        public string title;
        public string content;
        public string rewardType;
        public int rewardAmount;
        public bool claimed;

        public string startTime; // ex: 2025-08-07T18:00:00+09:00
        public string endTime;   // ex: 2025-08-07T18:59:59+09:00
    }
}
