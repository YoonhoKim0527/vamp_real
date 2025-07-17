using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Vampire
{
    public class BlackHoleSpawner : MonoBehaviour
    {
        [SerializeField] ObjectPool blackHolePool;
        [SerializeField] Tilemap groundTilemap;
        [SerializeField, Range(0f, 1f)] float spawnChance = 0.03f;
        [SerializeField] bool spawnEnabled = true;

        public void SpawnBlackHoleAtTile(Vector3Int tileCell)
        {
            if (!spawnEnabled) return;
            if (Random.value > spawnChance) return;

            Vector2 offset = Random.insideUnitCircle * 3f;
            Vector3 tileWorldPos = groundTilemap.GetCellCenterWorld(tileCell);
            Vector3 spawnPos = tileWorldPos + new Vector3(offset.x, offset.y, 0);

            GameObject blackHole = blackHolePool.Get();
            blackHole.transform.position = spawnPos;

            float scale = Random.Range(1f, 3f);
            blackHole.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}
