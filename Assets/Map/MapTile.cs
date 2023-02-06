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
            float widthPerSpawn = width / (spawnWidth + 2);
            float heightPerSpawn = height / (spawnHeight + 2);

            Vector3 root = zone.transform.position - transform.right * width / 2 - transform.forward * height / 2;

            for (int x = 1; x <= spawnWidth; x++)
            {
                for (int y = 1; y <= spawnHeight; y++)
                {
                    spawns.Add(new SpawnTransform
                    {
                        rotation = zone.transform.rotation,
                        halfExtents = new Vector2(widthPerSpawn, heightPerSpawn) / 2,
                        position = root + transform.right * widthPerSpawn * x + transform.forward * heightPerSpawn * y,

                    });

                }
            }


        }
        return spawns;
    }
}
