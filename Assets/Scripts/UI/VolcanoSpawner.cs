using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Vampire
{
    public class VolcanoSpawner : MonoBehaviour
    {
        [SerializeField] ObjectPool volcanoPool;
        [SerializeField] Tilemap groundTilemap;
        [SerializeField, Range(0f, 1f)] float volcanoSpawnChance = 0.05f;
        [SerializeField] bool spawnEnabled = true;

        public void SpawnVolcanoAtTile(Vector3Int tileCell)
        {
            if (!spawnEnabled) return;
            if (Random.value > volcanoSpawnChance) return;
            Vector2 offset = Random.insideUnitCircle * 3f;
            Vector3 tileWorldPos = groundTilemap.GetCellCenterWorld(tileCell);
            Vector3 spawnPos = tileWorldPos + new Vector3(offset.x, offset.y, 0);

            GameObject volcano = volcanoPool.Get();
            volcano.transform.position = spawnPos;

            float scale = Random.Range(0.8f, 1.5f);
            volcano.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}

