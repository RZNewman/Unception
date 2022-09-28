using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Utils;

public class MapGenerator : NetworkBehaviour
{
    public TileWeight[] tilesPre;
    public GameObject doorPre;
    public GameObject endPre;
    public GameObject stitchPre;
    public GameObject floorRootPre;


    public int tilesPerFloor = 30;
    float currentFloorScale = 1f;
    float currentFloorPower;


    GameObject currentFloor;
    GameObject lastFloor;

    Vector3 floorOffset = Vector3.zero;

    [System.Serializable]
    public struct TileWeight
    {
        public GameObject prefab;
        public float weight;
    }






    MonsterSpawn spawner;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            spawner = GetComponent<MonsterSpawn>();
            buildNewLevel(transform.position, 1000);
        }

    }
    static List<TileWeight> normalizeWeights(List<TileWeight> weights)
    {
        float total = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            total += weights[i].weight;
        }
        for (int i = 0; i < weights.Count; i++)
        {
            TileWeight w = weights[i];
            w.weight = w.weight / total;
            weights[i] = w;
        }
        return weights;
    }

    public void endOfLevel(Vector3 worldPos, float power)
    {
        spawner.upDifficulty();
        buildNewLevel(worldPos, power);
    }

    void buildNewLevel(Vector3 worldPos, float power)
    {
        currentFloorPower = power;
        currentFloorScale = Power.scale(power);
        spawner.setSpawnPower(power);

        lastFloor = currentFloor;
        floorOffset = worldPos - transform.position;
        buildGrid();
    }
    public void cleanupLevel()
    {
        Destroy(lastFloor);
    }


    void buildGrid()
    {

        currentFloor = Instantiate(floorRootPre, transform.position + floorOffset, Quaternion.identity, transform);
        spawner.setFloor(currentFloor.transform);
        currentFloor.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(currentFloor);

        List<GameObject> tiles = new List<GameObject>();
        List<GameObject> doors = new List<GameObject>();
        List<GameObject> badDoors = new List<GameObject>();
        System.Action<TileDelta> processDelta = (TileDelta delta) =>
        {
            tiles.Add(delta.tile);
            foreach (GameObject d in delta.removed)
            {
                doors.Remove(d);
                GameObject s = Instantiate(stitchPre, d.transform.position, d.transform.rotation, currentFloor.transform);
                s.transform.localScale = Vector3.one * currentFloorScale;
                s.GetComponent<ClientAdoption>().parent = currentFloor;
                RaycastHit hit;
                Physics.Raycast(
                    s.transform.position + s.transform.forward * -2f * currentFloorScale + Vector3.up * 2f * currentFloorScale,
                    Vector3.down,
                    out hit,
                    6f * currentFloorScale,
                    LayerMask.GetMask("Terrain"));
                float start = hit.point.y - s.transform.position.y;

                Physics.Raycast(
                    s.transform.position + s.transform.forward * 2f * currentFloorScale + Vector3.up * 2f * currentFloorScale,
                    Vector3.down,
                    out hit,
                    6f * currentFloorScale,
                    LayerMask.GetMask("Terrain"));
                float end = hit.point.y - s.transform.position.y;
                NavMeshLink link = s.GetComponent<NavMeshLink>();
                link.startPoint = new Vector3(link.startPoint.x, start, link.startPoint.z);
                link.endPoint = new Vector3(link.endPoint.x, end, link.endPoint.z);
            }
            foreach (GameObject d in delta.added)
            {
                doors.Add(d);
            }
        };

        processDelta(buildTile(getTilePrefab(tilesPre.ToList()).prefab, new TilePlacement { position = currentFloor.transform.position, rotation = Quaternion.identity }));
        int tileCount = tilesPerFloor - 1;
        //TODO Coroutine
        for (int i = 0; i < tileCount && doors.Count > 0; i++)
        {
            //TODO Make level more linear
            GameObject door = doors.RandomItemWeighted();

            //Removes the door without trying, if the space right in front isnt free
            if (Physics.OverlapBox(
                door.transform.position + door.transform.forward * 5f * currentFloorScale + Vector3.up * 4f * currentFloorScale,
                new Vector3(5f, 4f, 5f) * 0.99f * currentFloorScale,
                door.transform.rotation,
                LayerMask.GetMask("MapTile")).Length > 0
                )
            {
                doors.Remove(door);
                badDoors.Add(door);
                i--;
                continue;
            }
            List<TileWeight> weights;
            if (i == tileCount - 1)
            {
                weights = new List<TileWeight>();
                weights.Add(new TileWeight { prefab = endPre, weight = 1 });
            }
            else
            {
                weights = tilesPre.ToList();
            }

            bool didCreate = false;
            while (weights.Count > 0)
            {
                TileWeight weight = getTilePrefab(weights);
                TilePlacement place = checkTile(door, weight.prefab);
                if (place.success)
                {
                    processDelta(buildTile(weight.prefab, place));
                    didCreate = true;
                    break;
                }
                else
                {
                    weights.Remove(weight);
                }

            }

            if (didCreate)
            {
                continue;
            }
            else
            {
                doors.Remove(door);
                badDoors.Add(door);
                i--;
            }



        }

        foreach (GameObject hole in doors)
        {
            buildDoorBlocker(hole.transform.position, hole.transform.rotation);
        }
        foreach (GameObject hole in badDoors)
        {
            buildDoorBlocker(hole.transform.position, hole.transform.rotation);
        }

        spawner.spawnLevel(tiles);
    }

    struct TilePlacement
    {
        public bool success;
        public Vector3 position;
        public Quaternion rotation;
    }
    struct TileDelta
    {
        public GameObject tile;
        public List<GameObject> added;
        public List<GameObject> removed;
    }
    static TileWeight getTilePrefab(List<TileWeight> weights)
    {
        weights = normalizeWeights(weights);
        float value = Random.value;
        int index = 0;
        while (index < weights.Count && weights[index].weight < value)
        {
            value -= weights[index].weight;
            index++;
        }
        if (index == weights.Count)
        {
            index--;
        }
        return weights[index];
    }
    TilePlacement checkTile(GameObject door, GameObject tilePrefab)
    {
        Vector3 position;
        Quaternion rotation;

        List<GameObject> doorsPre = tilePrefab.GetComponent<MapTile>().Doors();
        while (doorsPre.Count > 0)
        {
            GameObject doorPre = doorsPre.RandomItem();
            Vector3 localDiff = -doorPre.transform.position;
            rotation = Quaternion.LookRotation(-Vector3.forward) * Quaternion.Inverse(Quaternion.LookRotation(doorPre.transform.forward)) * Quaternion.LookRotation(door.transform.forward);
            Vector3 worldDiff = rotation * localDiff * currentFloorScale;
            position = door.transform.position + worldDiff;

            if (checkTileZones(tilePrefab, position, rotation))
            {
                return new TilePlacement { success = true, position = position, rotation = rotation };
            }

            doorsPre.Remove(doorPre);
        }



        return new TilePlacement { success = false };
    }
    bool checkTileZones(GameObject tilePrefab, Vector3 position, Quaternion rotation)
    {
        List<GameObject> zonesPre = tilePrefab.GetComponent<MapTile>().Zones();
        foreach (GameObject zone in zonesPre)
        {
            if (Physics.OverlapBox(position + rotation * zone.transform.position * currentFloorScale, zone.transform.lossyScale * 0.5f * 0.99f * currentFloorScale, rotation * zone.transform.rotation, LayerMask.GetMask("MapTile")).Length > 0)
            {
                return false;
            }
        }
        return true;
    }
    TileDelta buildTile(GameObject tilePrefab, TilePlacement place)
    {
        GameObject t = Instantiate(tilePrefab, place.position, place.rotation, currentFloor.transform);
        t.transform.localScale = Vector3.one * currentFloorScale;

        t.GetComponent<ClientAdoption>().parent = currentFloor;
        NextLevel lvl = t.GetComponentInChildren<NextLevel>();
        if (lvl)
        {
            lvl.setGen(this);
        }
        NetworkServer.Spawn(t);
        Physics.SyncTransforms();

        List<GameObject> doors = t.GetComponent<MapTile>().Doors();
        List<GameObject> added = new List<GameObject>();
        List<GameObject> removed = new List<GameObject>();
        foreach (GameObject doorInst in doors)
        {
            Collider[] found = Physics.OverlapSphere(doorInst.transform.position, 1f, LayerMask.GetMask("Doors"));
            if (found.Length > 1)
            {
                List<GameObject> foundDoors = found.Select(c => c.gameObject).ToList();
                foundDoors.Remove(doorInst);
                removed.Add(foundDoors[0]);
            }
            else
            {
                added.Add(doorInst);
            }

        }
        return new TileDelta { tile = t, added = added, removed = removed };
    }
    void buildDoorBlocker(Vector3 position, Quaternion rotation)
    {
        GameObject t = Instantiate(doorPre, position, rotation, currentFloor.transform);
        t.transform.localScale = Vector3.one * currentFloorScale;
        t.GetComponent<ClientAdoption>().parent = currentFloor;
        NetworkServer.Spawn(t);
    }





}

