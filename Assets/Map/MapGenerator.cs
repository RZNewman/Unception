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
using idbrii.navgen;
using UnityEditor;

public class MapGenerator : NetworkBehaviour
{
    public TileWeight[] tilesPre;
    public GameObject doorPre;
    public GameObject endPre;
    public GameObject stitchPre;
    public GameObject floorRootPre;


    float currentFloorScale = 1f;
    GameObject currentFloor;
    Atlas atlas;
    NavMeshDataInstance navData = new NavMeshDataInstance();
    List<NavMeshLink> navLinks = new List<NavMeshLink>();
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
        atlas = FindObjectOfType<Atlas>();
        sound = FindObjectOfType<SoundManager>();
        if (isServer)
        {
            spawner = GetComponent<MonsterSpawn>();
        }

    }

    Map currentMap
    {
        get
        {
            return atlas.currentMap;
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

    public IEnumerator buildMap()
    {
        currentFloorScale = Power.scalePhysical(currentMap.power);
        currentFloorIndex = 0;
        spawner.setSpawnPower(currentMap.power);
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
        int packCount = currentMap.floors[currentFloorIndex].packs + currentMap.floors[currentFloorIndex].encounters.Length + breakablesPerFloor;
        //increase packs to make sure not every location is populated
        packCount = Mathf.FloorToInt(packCount * currentMap.floors[currentFloorIndex].sparseness);
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
        agent.agentHeight = 1.5f * currentFloorScale;
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(currentFloor.transform, LayerMask.GetMask("Terrain"), NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        navData = NavMesh.AddNavMeshData(NavMeshBuilder.BuildNavMeshData(agent, sources, new Bounds(Vector3.zero, Vector3.one * 4000), Vector3.zero, Quaternion.identity));
        NavLinkGenerator linkGenerator = ScriptableObject.CreateInstance<NavLinkGenerator>();
        linkGenerator.m_FallLinkPrefab = stitchPre.transform;
        linkGenerator.m_JumpLinkPrefab = stitchPre.transform;
        linkGenerator.m_MaxVerticalJump = 0;
        linkGenerator.m_MaxHorizontalJump = 5 * currentFloorScale;
        linkGenerator.m_MaxVerticalFall = 15 * currentFloorScale;
        linkGenerator.m_PhysicsMask = LayerMask.GetMask("Terrain");
        linkGenerator.m_AgentRadius = agent.agentRadius;
        linkGenerator.m_AgentHeight = agent.agentHeight;
        GenerateLinks(linkGenerator);
        yield return null;

        yield return spawner.spawnLevel(spawnLocations, currentMap.floors[currentFloorIndex].packs, currentMap.difficulty, currentMap.floors[currentFloorIndex].encounters);
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


    // Nav link Gen Editor code
    #region NavLinkGenerator
    void GenerateLinks(NavLinkGenerator gen)
    {
        var tri = NavMesh.CalculateTriangulation();
        var edge_list = CreateEdges(tri);
        foreach (var edge in edge_list)
        {
            edge.ComputeDerivedData();
        }
        if (edge_list.Count() == 0)
        {
            return;
        }

        RemoveLinks();
        navLinks.Clear();
        Transform parent = currentFloor.transform;

        foreach (var edge in edge_list)
        {
            var mid = edge.GetMidpoint();
            var fwd = edge.m_Normal;
            var link = CreateNavLink(parent, gen, edge, mid, fwd);
            if (link != null)
            {
                navLinks.Add(link);
            }
        }
    }


    NavMeshLink CreateNavLink(Transform parent, NavLinkGenerator gen, NavEdge edge, Vector3 mid, Vector3 fwd)
    {
        RaycastHit phys_hit;
        RaycastHit ignored;
        NavMeshHit nav_hit;
        var ground_found = Color.Lerp(Color.red, Color.white, 0.75f);
        var ground_missing = Color.Lerp(Color.red, Color.white, 0.35f);
        var navmesh_found = Color.Lerp(Color.cyan, Color.white, 0.75f);
        var navmesh_missing = Color.Lerp(Color.red, Color.white, 0.65f);
        var traverse_clear = Color.green;
        var traverse_hit = Color.red;
        for (int i = 0; i < gen.m_Steps; ++i)
        {
            float scale = (float)i / (float)gen.m_Steps;

            var top = mid + (fwd * gen.m_MaxHorizontalJump * scale);
            var down = top + (Vector3.down * gen.m_MaxVerticalFall);
            bool hit = Physics.Linecast(top, down, out phys_hit, gen.m_PhysicsMask.value, QueryTriggerInteraction.Ignore);
            //~ Debug.DrawLine(mid, top, hit ? ground_found : ground_missing, k_DrawDuration);
            //~ Debug.DrawLine(top, down, hit ? ground_found : ground_missing, k_DrawDuration);
            if (hit)
            {
                var max_distance = gen.m_MaxVerticalFall - phys_hit.distance;
                hit = NavMesh.SamplePosition(phys_hit.point, out nav_hit, max_distance, gen.m_NavMask);
                // Only place downward links (to avoid double placement).
                hit = hit && (nav_hit.position.y <= mid.y);
                bool is_original_edge = edge.IsPointOnEdge(nav_hit.position);
                hit &= !is_original_edge; // don't count self
                                          //~ Debug.DrawLine(phys_hit.point, nav_hit.position, hit ? navmesh_found : navmesh_missing, k_DrawDuration);
                if (hit)
                {
                    var height_offset = Vector3.up * gen.m_AgentHeight;
                    var transit_start = mid + height_offset;
                    var transit_end = nav_hit.position + height_offset;
                    // Raycast both ways to ensure we're not inside a collider.

                    hit = Physics.Linecast(transit_start, transit_end, out ignored, gen.m_PhysicsMask.value, QueryTriggerInteraction.Ignore)
                        || Physics.Linecast(transit_end, transit_start, out ignored, gen.m_PhysicsMask.value, QueryTriggerInteraction.Ignore);
                    //~ Debug.DrawLine(transit_start, transit_end, hit ? traverse_clear : traverse_hit, k_DrawDuration);
                    if (hit)
                    {
                        // Agent can't jump through here.
                        continue;
                    }
                    var height_delta = nav_hit.position.y - mid.y;
                    var prefab = gen.m_JumpLinkPrefab;
                    if (height_delta > gen.m_MaxVerticalJump)
                    {
                        prefab = gen.m_FallLinkPrefab;
                    }
                    var t = PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene) as Transform;
                    Debug.Assert(t != null, $"Failed to instantiate {prefab}");
                    t.SetParent(parent);
                    t.SetPositionAndRotation(mid, edge.m_Away);
                    var link = t.GetComponent<NavMeshLink>();

                    // Push endpoint out into the navmesh to ensure good
                    // connection. Necessary to prevent invalid links.
                    var inset = 0.05f;
                    link.startPoint = link.transform.InverseTransformPoint(mid - fwd * inset);
                    link.endPoint = link.transform.InverseTransformPoint(nav_hit.position) + (Vector3.forward * inset);
                    link.width = edge.m_Length;
                    link.UpdateLink();
                    //Debug.Log("Created NavLink");
                    Undo.RegisterCompleteObjectUndo(link.gameObject, "Create NavMeshLink");


                    // Attach a component that has the information we
                    // used to decide how to create this navlink. Much
                    // easier to go back and inspect it like this than
                    // to try to examine the output as you generate
                    // navlinks. Mostly useful for debugging
                    // NavLinkGenerator.
                    //var reason = link.gameObject.AddComponent<NavLinkCreationReason>();
                    //reason.gen = gen;
                    //reason.fwd = fwd;
                    //reason.mid = mid;
                    //reason.top = top;
                    //reason.down = down;
                    //reason.transit_start = transit_start;
                    //reason.transit_end = transit_end;
                    //reason.nav_hit_position = nav_hit.position;
                    //reason.phys_hit_point = phys_hit.point;


                    return link;
                }
            }
        }
        return null;
    }

    void RemoveLinks()
    {
        var nav_links = currentFloor.GetComponentsInChildren<NavMeshLink>();
        foreach (var link in nav_links)
        {
            GameObject.DestroyImmediate(link.gameObject);
        }
    }
    static NavEdge TriangleToEdge(NavMeshTriangulation tri, int start, int end)
    {
        var v1 = tri.vertices[tri.indices[start]];
        var v2 = tri.vertices[tri.indices[end]];
        return new NavEdge(v1, v2);
    }

    static IEnumerable<NavEdge> CreateEdges(NavMeshTriangulation tri)
    {
        // use HashSet to ignore duplicate edges.
        var edges = new HashSet<NavEdge>(new NavEdgeEqualityComparer());
        for (int i = 0; i < tri.indices.Length - 1; i += 3)
        {
            AddIfUniqueAndRemoveIfNot(edges, TriangleToEdge(tri, i, i + 1));
            AddIfUniqueAndRemoveIfNot(edges, TriangleToEdge(tri, i + 1, i + 2));
            AddIfUniqueAndRemoveIfNot(edges, TriangleToEdge(tri, i + 2, i));
        }
        return edges;
    }

    static void AddIfUniqueAndRemoveIfNot(HashSet<NavEdge> set, NavEdge edge)
    {
        bool had_edge = set.Remove(edge);
        if (!had_edge)
        {
            set.Add(edge);
        }
    }
    static float DistanceSqToPointOnLine(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        Vector3 pa = a - p;
        var mag = ab.magnitude;
        Vector3 c = ab * (Vector3.Dot(pa, ab) / (mag * mag));
        Vector3 d = pa - c;
        return Vector3.Dot(d, d);
    }

    class NavEdge
    {
        public Vector3 m_StartPos;
        public Vector3 m_EndPos;

        // Derived data
        public float m_Length;
        public Vector3 m_Normal;
        public Quaternion m_Away;

        public NavEdge(Vector3 start, Vector3 end)
        {
            m_StartPos = start;
            m_EndPos = end;
        }

        public Vector3 GetMidpoint()
        {
            return Vector3.Lerp(m_StartPos, m_EndPos, 0.5f);
        }

        public void ComputeDerivedData()
        {
            m_Length = Vector3.Distance(m_StartPos, m_EndPos);
            var normal = Vector3.Cross(m_EndPos - m_StartPos, Vector3.up).normalized;

            // Point it outside the nav poly.
            NavMeshHit nav_hit;
            var mid = GetMidpoint();
            var end = mid - normal * 0.3f;
            bool hit = NavMesh.SamplePosition(end, out nav_hit, 0.2f, NavMesh.AllAreas);
            //~ Debug.DrawLine(mid, end, hit ? Color.red : Color.white);
            if (!hit)
            {
                normal *= -1f;
            }
            m_Normal = normal;
            m_Away = Quaternion.LookRotation(normal);
        }
        public bool IsPointOnEdge(Vector3 point)
        {
            return DistanceSqToPointOnLine(m_StartPos, m_EndPos, point) < 0.001f;
        }
    }
    class NavEdgeEqualityComparer : IEqualityComparer<NavEdge>
    {
        public bool Equals(NavEdge lhs, NavEdge rhs)
        {
            return
                (lhs.m_StartPos == rhs.m_StartPos && lhs.m_EndPos == rhs.m_EndPos)
                || (lhs.m_StartPos == rhs.m_EndPos && lhs.m_EndPos == rhs.m_StartPos);
        }

        public int GetHashCode(NavEdge e)
        {
            return e.m_StartPos.GetHashCode() ^ e.m_EndPos.GetHashCode();
        }
    }

    #endregion
}

