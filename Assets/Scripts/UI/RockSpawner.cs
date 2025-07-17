using UnityEngine;
using UnityEngine.Tilemaps;

namespace Vampire
{
    public class RockSpawner : MonoBehaviour
    {
        [SerializeField] ObjectPool rockPool;
        [SerializeField] Tilemap groundTilemap;
        [SerializeField] bool spawnEnabled = true;

        public void SpawnRocksAtTile(Vector3Int tileCell)
        {
            if (!spawnEnabled) return;
            Vector3 tileWorldPos = groundTilemap.GetCellCenterWorld(tileCell);
            int count = Random.Range(1, 4);

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 3f;
                Vector3 spawnPos = tileWorldPos + new Vector3(offset.x, offset.y, 0);

                GameObject rock = rockPool.Get();
                rock.transform.position = spawnPos;

                // 회전 + 크기 조절
                float scale = Random.Range(0.5f, 1.8f);
                rock.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
    }
}