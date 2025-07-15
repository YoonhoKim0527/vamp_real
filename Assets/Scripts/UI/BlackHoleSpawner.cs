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

        public void SpawnBlackHoleAtTile(Vector3Int tileCell)
        {
            if (Random.value > spawnChance) return;

            Vector3 tileWorldPos = groundTilemap.GetCellCenterWorld(tileCell);

            GameObject blackHole = blackHolePool.Get();
            blackHole.transform.position = tileWorldPos;

            float scale = Random.Range(0.8f, 1.3f);
            blackHole.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}
