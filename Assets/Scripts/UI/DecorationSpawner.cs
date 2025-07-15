using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vampire
{
public class DecorationSpawner : MonoBehaviour
{
    GameObject[] decorationPrefabs;
    public float spawnRadius = 10f;
    public float minDistanceBetween = 2f;
    public int maxSpawnPerCheck = 3;
    public Transform player;
    private List<Vector3> spawnedPositions = new();

    public void SetDecorationPrefabs(GameObject[] prefabs)
    {
        decorationPrefabs = prefabs;
    }

    void Update()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0 || player == null)
            return;

        for (int i = 0; i < maxSpawnPerCheck; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = player.position + new Vector3(offset.x, offset.y, 0);

            if (IsFarFromExisting(pos))
            {
                GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
                Instantiate(prefab, pos, Quaternion.identity);
                spawnedPositions.Add(pos);
            }
        }
    }

    bool IsFarFromExisting(Vector3 pos)
    {
        foreach (var p in spawnedPositions)
        {
            if (Vector3.Distance(p, pos) < minDistanceBetween)
                return false;
        }
        return true;
    }
}
}
