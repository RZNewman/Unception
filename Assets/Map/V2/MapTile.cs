using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MonsterSpawn;

public class MapTile : MonoBehaviour
{


    public List<Door> Doors()
    {
        return gameObject.GetComponentsInChildren<Door>().ToList();
    }

    public List<GameObject> Zones()
    {
        return gameObject.ChildrenWithTag("MapTile");
    }

    public List<SpawnTransform> Spawns(float scale)
    {
        List<GameObject> zones = gameObject.ChildrenWithTag("Spawn");
        List<SpawnTransform> spawns = new List<SpawnTransform>();
        foreach (GameObject zone in zones)
        {
            float width = zone.transform.lossyScale.x;
            float height = zone.transform.lossyScale.z;
            int spawnWidth = Mathf.Max(Mathf.RoundToInt(width / (lengthPerPack * scale)), 1);
            int spawnHeight = Mathf.Max(Mathf.RoundToInt(height / (lengthPerPack * scale)), 1);
            float widthPerSpawn = width / (spawnWidth);
            float heightPerSpawn = height / (spawnHeight);

            Vector3 root = zone.transform.position - zone.transform.right * width / 2 - zone.transform.forward * height / 2;
            Vector3 halfTile = zone.transform.right * widthPerSpawn / 2 + zone.transform.forward * heightPerSpawn / 2;

            for (int x = 0; x < spawnWidth; x++)
            {
                for (int y = 0; y < spawnHeight; y++)
                {
                    spawns.Add(new SpawnTransform
                    {
                        rotation = zone.transform.rotation,
                        halfExtents = new Vector3(widthPerSpawn, zone.transform.lossyScale.y, heightPerSpawn) / 2,
                        position = root + halfTile + zone.transform.right * widthPerSpawn * x + zone.transform.forward * heightPerSpawn * y,

                    });

                }
            }


        }
        return spawns;
    }
}
