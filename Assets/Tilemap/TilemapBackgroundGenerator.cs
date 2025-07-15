using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

    public class TilemapBackgroundGenerator : MonoBehaviour
    {
        [SerializeField] Tilemap tilemap;
        [SerializeField] TileBase[] groundTiles; // 현재 레벨 타일 리스트
        [SerializeField] Transform player;
        [SerializeField] int loadRadius = 10;
        [SerializeField] RockSpawner rockSpawner;
        [SerializeField] VolcanoSpawner volcanoSpawner;

        Dictionary<Vector3Int, TileBase> placedTiles = new();
        Vector3Int lastPlayerCell;


        public void Init(TileBase[] levelTiles, Transform playerTransform)
        {
            groundTiles = levelTiles;
            player = playerTransform;

            // ✅ 초기 플레이어 셀 위치 저장
            lastPlayerCell = tilemap.WorldToCell(player.position);

            // ✅ 바로 주변 타일 생성
            UpdateTilesAroundPlayer();
        }
        void Start()
        {
            lastPlayerCell = tilemap.WorldToCell(player.position);
        }

        void Update()
        {
            Vector3Int currentCell = tilemap.WorldToCell(player.position);
            if ((currentCell - lastPlayerCell).sqrMagnitude >= 1)
            {
                UpdateTilesAroundPlayer();
                lastPlayerCell = currentCell;
            }
        }

        void UpdateTilesAroundPlayer()
        {
            Vector3Int centerCell = tilemap.WorldToCell(player.position);

            for (int x = -loadRadius; x <= loadRadius; x++)
                for (int y = -loadRadius; y <= loadRadius; y++)
                {
                    Vector3Int pos = centerCell + new Vector3Int(x, y, 0);
                    if (!placedTiles.ContainsKey(pos))
                    {
                        TileBase tile = groundTiles[Random.Range(0, groundTiles.Length)];
                        tilemap.SetTile(pos, tile);
                        placedTiles[pos] = tile;
                        rockSpawner.SpawnRocksAtTile(pos);
                        volcanoSpawner.SpawnVolcanoAtTile(pos);
                    }
                }
        }
}

}
