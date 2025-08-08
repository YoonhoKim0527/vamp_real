using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
    public class BossLaserSkill : BossAbility
    {
        public GameObject warningLaserPrefab;
        public GameObject realLaserPrefab;

        public float warningInterval = 5f;
        public float blinkInterval = 0.5f;
        public int blinkCount = 3;

        private Coroutine skillRoutine;

        public override IEnumerator Activate()
        {
            active = true;
            yield return LaserAttackOnce(); // 이걸 직접 기다리게 하면 Act()가 정상 동작함
        }

        private void FixedUpdate()
        {
            if (active && monster != null && playerCharacter != null)
            {
                Vector2 moveDirection = (playerCharacter.transform.position - monster.transform.position).normalized;
                monster.Move(moveDirection, Time.fixedDeltaTime);
                entityManager.Grid.UpdateClient(monster);
            }
        }


        private IEnumerator LaserAttackOnce()
        {
            yield return new WaitForSeconds(warningInterval);

            List<Vector2> positions = new List<Vector2>();
            List<LaserDirection> directions = new List<LaserDirection>();

            Debug.Log("[BOSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS]");

            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = GetRandomPositionOnScreen();
                LaserDirection dir = GetRandomDirection();

                positions.Add(pos);
                directions.Add(dir);

                GameObject warnLaser = Instantiate(warningLaserPrefab);
                warnLaser.GetComponent<LaserWarning>().Init(pos, dir, blinkInterval, blinkCount);
            }

            yield return new WaitForSeconds(blinkInterval * blinkCount + 1f);

            for (int i = 0; i < 3; i++)
            {
                GameObject realLaser = Instantiate(realLaserPrefab);
                realLaser.GetComponent<RealLaser>().Init(positions[i], directions[i]);
            }

            yield return new WaitForSeconds(1f); // 쿨다운 후 종료
        }

        public override void Deactivate()
        {
            base.Deactivate();
            if (skillRoutine != null)
            {
                StopCoroutine(skillRoutine);
            }
        }

        private void FixedUpdate()
        {
            if (active && monster != null && playerCharacter != null)
            {
                Vector2 moveDirection = (playerCharacter.transform.position - monster.transform.position).normalized;
                monster.Move(moveDirection, Time.fixedDeltaTime);
                entityManager.Grid.UpdateClient(monster);
            }
        }
        
        private Vector2 GetRandomPositionOnScreen()
        {
            Vector2 screenMin = Camera.main.ViewportToWorldPoint(Vector2.zero);
            Vector2 screenMax = Camera.main.ViewportToWorldPoint(Vector2.one);
            return new Vector2(Random.Range(screenMin.x, screenMax.x), Random.Range(screenMin.y, screenMax.y));
        }

        private LaserDirection GetRandomDirection()
        {
            int idx = Random.Range(0, 4);
            return (LaserDirection)idx;
        }

        public override float Score()
        {
            return 1f; // 현재는 무조건 1.0을 반환 (정상)
        }
    }
}