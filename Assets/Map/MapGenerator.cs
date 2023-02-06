using Mirror;
using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Atlas;
using static Utils;
using static MonsterSpawn;

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
    NavMeshDataInstance navData = new NavMeshDataInstance();
    int currentFloorIndex;


    [System.Serializable]
    public struct TileWeight
    {
        public GameObject prefab;
        public float weight;
    }






    MonsterSpawn spawner;
    SoundManager sound;
    // Start is called before the first frame update
    void Start()
    {
        sound = FindObjectOfType<SoundManager>();
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

    public void endFloor(Vector3 position)
    {
        StartCoroutine(endFloorRoutine(position));
    }
    public void destroyFloor()
    {
        Destroy(currentFloor);
    }
    float portalTime = 1.5f;
    IEnumerator endFloorRoutine(Vector3 position)
    {
        sound.sendSoundDuration(SoundManager.SoundClip.PortalStart, position, portalTime);
        yield return new WaitForSecondsRealtime(portalTime);
        sound.sendSound(SoundManager.SoundClip.PortalEnd, position);
        PlayerGhost[] ghosts = FindObjectsOfType<PlayerGhost>();
        GameObject[] units = ghosts.Select(g => g.unit).ToArray();
        foreach (GameObject playerUnit in units)
        {
            playerUnit.transform.position = transform.position + Vector3.up * 5;
            playerUnit.SetActive(false);
        }
        foreach (PlayerGhost ghost in ghosts)
        {
            ghost.RpcSetAudio(true);
            ghost.transform.position = position;
        }

        yield return new WaitForSecondsRealtime(portalTime);
        Destroy(currentFloor);
        yield return null;
        currentFloorIndex++;

        if (currentFloorIndex >= currentMap.floors.Length)
        {
            FindObjectOfType<Atlas>().disembark();
        }
        else
        {
            foreach (PlayerGhost ghost in ghosts)
            {
                ghost.transform.position = transform.position;
                ghost.RpcSetAudio(false);
                ghost.refreshLives();
            }
            yield return buildGridRoutine();
            foreach (GameObject playerUnit in units)
            {
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
    enum TileDistanceMode
    {
        tiles,
        direction
    }

    IEnumerator buildGridRoutine()
    {

        currentFloor = Instantiate(floorRootPre, transform.position, Quaternion.identity, transform);
        spawner.setFloor(currentFloor.transform);
        currentFloor.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(currentFloor);

        List<GameObject> tiles = new List<GameObject>();
        List<SpawnTransform> spawnLocations = new List<SpawnTransform>();
        SimplePriorityQueue<Door> doors = new SimplePriorityQueue<Door>();
        List<Door> badDoors = new List<Door>();

        TileDistanceMode mode = TileDistanceMode.direction;
        Vector3 tileDirection = new Vector3(Random.value.asRange(-1, 1), 0, Random.value.asRange(-1, 1)).normalized;

        System.Action<TileDelta> processDelta = (TileDelta delta) =>
        {
            tiles.Add(delta.tile);
            float dist = 0;
            bool distSet = false;
            if (!delta.skipZones)
            {
                spawnLocations.AddRange(delta.locations);
            }

            foreach (Door d in delta.removed)
            {
                if (mode == TileDistanceMode.tiles)
                {
                    if (distSet)
                    {
                        dist = Mathf.Min(dist, d.floorDist);
                    }
                    else
                    {
                        dist = d.floorDist;
                        distSet = true;
                    }
                }
                doors.Remove(d);
                //buildPathStitch(d);
            }
            foreach (Door d in delta.added)
            {
                if (mode == TileDistanceMode.tiles)
                {
                    dist += 1;
                    d.floorDist = dist;
                    //TODO recalc other tile dists
                }
                else
                {
                    dist = Vector3.Dot(delta.tile.transform.localPosition, tileDirection);
                }
                doors.Enqueue(d, dist);

            }
        };
        TileDelta delta = buildTile(getTilePrefab(tilesPre.ToList()).prefab, new TilePlacement { position = currentFloor.transform.position, rotation = Quaternion.identity });
        delta.skipZones = true;
        processDelta(delta);
        //int tileCount = currentMap.floors[currentFloorIndex].tiles - 1;
        int packCount = currentMap.floors[currentFloorIndex].packs + chestPerFloor + potPerFloor;
        //increase packs to make sure not every location is populated
        packCount = Mathf.FloorToInt(packCount * 1.5f);
        bool ending = false;

        while ((spawnLocations.Count < packCount || ending) && doors.Count > 0)
        {

            Door door;
            if (ending)
            {
                door = doors.Last();
            }
            else
            {
                float weight = 2;
                switch (mode)
                {
                    case TileDistanceMode.tiles:
                        weight = 7;
                        break;
                    case TileDistanceMode.direction:
                        weight = 9f;
                        break;

                }
                door = doors.RandomItemWeighted(weight);
            }

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
                continue;
            }
            List<TileWeight> weights;
            if (ending)
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
                    delta = buildTile(weight.prefab, place);
                    delta.skipZones = ending;
                    processDelta(delta);
                    didCreate = true;
                    break;
                }
                else
                {
                    weights.Remove(weight);
                }

            }

            if (!didCreate)
            {
                doors.Remove(door);
                badDoors.Add(door);
                continue;
            }

            if (ending)
            {
                ending = false;
            }
            else if (spawnLocations.Count >= packCount)
            {
                ending = true;
            }

            yield return null;

        }

        foreach (Door hole in doors)
        {
            if (hole.needsDoor)
            {
                buildDoorBlocker(hole.transform.position, hole.transform.rotation);
            }

        }
        foreach (Door hole in badDoors)
        {
            if (hole.needsDoor)
            {
                buildDoorBlocker(hole.transform.position, hole.transform.rotation);
            }
        }

        if (navData.valid)
        {
            NavMesh.RemoveNavMeshData(navData);
        }
        NavMeshBuildSettings agent = NavMesh.GetSettingsByID(0);
        agent.agentRadius = 0.6f * currentFloorScale;
        agent.agentClimb = 0.3f * currentFloorScale;
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(currentFloor.transform, LayerMask.GetMask("Terrain"), NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        navData = NavMesh.AddNavMeshData(NavMeshBuilder.BuildNavMeshData(agent, sources, new Bounds(Vector3.zero, Vector3.one * 4000), Vector3.zero, Quaternion.identity));

        yield return spawner.spawnLevel(spawnLocations, currentMap.floors[currentFloorIndex].packs, currentMap.difficulty);
        FindObjectsOfType<PlayerGhost>().ToList().ForEach(ghost => ghost.RpcSetCompassDirection(tileDirection));
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
        public List<Door> added;
        public List<Door> removed;
        public List<SpawnTransform> locations;
        public bool skipZones;
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
    TilePlacement checkTile(Door door, GameObject tilePrefab)
    {
        Vector3 position;
        Quaternion rotation;

        List<Door> doorsPre = tilePrefab.GetComponent<MapTile>().Doors();
        while (doorsPre.Count > 0)
        {
            Door doorPre = doorsPre.RandomItem();
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
            if (Physics.OverlapBox(position + rotation * zone.transform.position * currentFloorScale, (zone.transform.lossyScale * 0.5f * 0.99f + new Vector3(0, 2, 0)) * currentFloorScale, rotation * zone.transform.rotation, LayerMask.GetMask("MapTile")).Length > 0)
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

        List<Door> doors = t.GetComponent<MapTile>().Doors();
        List<Door> added = new List<Door>();
        List<Door> removed = new List<Door>();
        foreach (Door doorInst in doors)
        {
            Collider[] found = Physics.OverlapSphere(doorInst.transform.position, 1f, LayerMask.GetMask("Doors"));
            if (found.Length > 1)
            {
                List<Door> foundDoors = found.Select(c => c.GetComponent<Door>()).ToList();
                foundDoors.Remove(doorInst);
                removed.Add(foundDoors[0]);
            }
            else
            {
                added.Add(doorInst);
            }

        }
        return new TileDelta { tile = t, added = added, removed = removed, locations = t.GetComponent<MapTile>().Spawns(currentFloorScale) };
    }
    void buildDoorBlocker(Vector3 position, Quaternion rotation)
    {
        GameObject t = Instantiate(doorPre, position, rotation, currentFloor.transform);
        t.transform.localScale = Vector3.one * currentFloorScale;
        t.GetComponent<ClientAdoption>().parent = currentFloor;
        NetworkServer.Spawn(t);
    }





}

