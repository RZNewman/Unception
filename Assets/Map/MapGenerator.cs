using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Atlas;
using static Utils;

public class MapGenerator : NetworkBehaviour
{
    public TileWeight[] tilesPre;
    public GameObject doorPre;
    public GameObject endPre;
    public GameObject stitchPre;
    public GameObject floorRootPre;


    float currentFloorScale = 1f;
    GameObject currentFloor;
    Map currentMap;
    int currentFloorIndex;


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

    public void endFloor()
    {
        StartCoroutine(endFloorRoutine());
    }
    public void destroyFloor()
    {
        Destroy(currentFloor);
    }

    IEnumerator endFloorRoutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        Destroy(currentFloor);
        yield return null;
        currentFloorIndex++;
        GameObject[] units = FindObjectsOfType<PlayerGhost>().Select(g => g.unit).ToArray();
        if (currentFloorIndex >= currentMap.floors.Length)
        {
            foreach (GameObject playerUnit in units)
            {
                Destroy(playerUnit);
                FindObjectOfType<Atlas>().disembark();
            }
        }
        else
        {
            foreach (GameObject playerUnit in units)
            {
                playerUnit.transform.position = transform.position + Vector3.up * 5;
                playerUnit.SetActive(false);
            }

            yield return buildGridRoutine();
            foreach (GameObject playerUnit in units)
            {
                playerUnit.GetComponent<PlayerGhost>().refreshLives();
                playerUnit.SetActive(true);
            }
        }
        

    }

    public IEnumerator buildMap(Map m)
    {
        currentFloorScale = Power.scale(m.power);
        currentMap = m;
        currentFloorIndex = 0;
        spawner.setSpawnPower(m.power);
        yield return buildGridRoutine();
    }


    IEnumerator buildGridRoutine()
    {

        currentFloor = Instantiate(floorRootPre, transform.position, Quaternion.identity, transform);
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
                buildPathStitch(d);
            }
            foreach (GameObject d in delta.added)
            {
                doors.Add(d);
            }
        };

        processDelta(buildTile(getTilePrefab(tilesPre.ToList()).prefab, new TilePlacement { position = currentFloor.transform.position, rotation = Quaternion.identity }));
        int tileCount = currentMap.floors[currentFloorIndex].tiles - 1;
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

            yield return null;

        }

        foreach (GameObject hole in doors)
        {
            buildDoorBlocker(hole.transform.position, hole.transform.rotation);
        }
        foreach (GameObject hole in badDoors)
        {
            buildDoorBlocker(hole.transform.position, hole.transform.rotation);
        }

        foreach(GameObject tile in tiles)
        {
            NavMeshSurface sur = tile.GetComponent<NavMeshSurface>();
            sur.BuildNavMesh();
            yield return null;
        }

        //remove the end tile from spawner
        tiles.RemoveAt(tiles.Count - 1);
        tiles.RemoveAt(0);
        yield return spawner.spawnLevel(tiles, currentMap.floors[currentFloorIndex].packs, currentMap.difficulty);
    }
    void buildPathStitch(GameObject door)
    {
        GameObject s = Instantiate(stitchPre, door.transform.position, door.transform.rotation, currentFloor.transform);
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
            if (Physics.OverlapBox(position + rotation * zone.transform.position * currentFloorScale, (zone.transform.lossyScale * 0.5f * 0.99f + new Vector3(0,2,0)) * currentFloorScale, rotation * zone.transform.rotation, LayerMask.GetMask("MapTile")).Length > 0)
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

