using UnityEngine;

namespace Vampire
{
    public class SplitTracker : MonoBehaviour
    {
        private BossMonster[] clones;
        private BossMonster original;

        public void Init(BossMonster[] clones, BossMonster original)
        {
            this.clones = clones;
            this.original = original;

            foreach (var clone in clones)
                clone.OnDeath += CheckAllDead;
        }

        private void CheckAllDead()
        {
            foreach (var clone in clones)
            {
                if (clone != null && !clone.IsDead)
                    return;
            }

            Destroy(original.gameObject); // 최종 소멸
        }
    }
}