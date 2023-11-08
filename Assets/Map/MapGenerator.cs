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
    public GameObject fallPre;
    public GameObject jumpPre;
    public GameObject floorRootPre;
    public GameObject endPortalPre;


    float currentFloorScale = 1f;
    GameObject currentFloor;
    Atlas atlas;
    NavMeshDataInstance navData = new NavMeshDataInstance();
    List<NavMeshLink> navLinks = new List<NavMeshLink>();
    int currentFloorIndex;

    bool ending = false;

    MonsterSpawn spawner;
    SoundManager sound;
    WFCGeneration wfc;
    // Start is called before the first frame update
    void Start()
    {
        atlas = FindObjectOfType<Atlas>();
        sound = FindObjectOfType<SoundManager>();
        wfc = GetComponent<WFCGeneration>();
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


    public void endFloor(Vector3 position)
    {
        if (!ending)
        {
            ending = true;
            StartCoroutine(endFloorRoutine(position));
        }

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

    public Vector3 playerSpawn
    {
        get
        {
            return wfc.generationData.start;
        }
    }

    IEnumerator buildGridRoutine()
    {
        ending = false;

        currentFloor = Instantiate(floorRootPre, transform.position, Quaternion.identity, transform);
        spawner.setFloor(currentFloor.transform);
        currentFloor.GetComponent<ClientAdoption>().parent = gameObject;
        currentFloor.transform.localScale = Vector3.one * currentFloorScale;
        NetworkServer.Spawn(currentFloor);


        GameObject wfcFloor = Instantiate(floorRootPre, transform.position, Quaternion.identity, currentFloor.transform);
        wfcFloor.GetComponent<ClientAdoption>().parent = currentFloor;
        NetworkServer.Spawn(wfcFloor);
        yield return wfc.generate(wfcFloor);

        GameObject p = Instantiate(endPortalPre, wfc.generationData.end, Quaternion.identity, currentFloor.transform);
        p.GetComponent<NextLevel>().setGen(this);
        Vector3 tileDirection = wfc.generationData.end - wfc.generationData.start;



        if (navData.valid)
        {
            NavMesh.RemoveNavMeshData(navData);
        }
        NavMeshBuildSettings agent = NavMesh.GetSettingsByID(0);
        agent.agentRadius = 0.6f * currentFloorScale;
        agent.agentClimb = 0.3f * currentFloorScale;
        agent.agentHeight = 1.5f * currentFloorScale;
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        //NavMeshBuilder.CollectSources(currentFloor.transform, LayerMask.GetMask("Terrain"), NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        foreach (GameObject tile in wfc.generationData.navTiles)
        {
            List<NavMeshBuildSource> sourcesTile = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(tile.transform, LayerMask.GetMask("Terrain"), NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sourcesTile);
            sources.AddRange(sourcesTile);
        }
        navData = NavMesh.AddNavMeshData(NavMeshBuilder.BuildNavMeshData(agent, sources, new Bounds(Vector3.zero, Vector3.one * 4000), Vector3.zero, Quaternion.identity));
        NavLinkGenerator linkGenerator = ScriptableObject.CreateInstance<NavLinkGenerator>();
        linkGenerator.m_FallLinkPrefab = fallPre.transform;
        linkGenerator.m_JumpLinkPrefab = jumpPre.transform;
        linkGenerator.m_MaxVerticalJump = 6 * currentFloorScale;
        linkGenerator.m_MaxHorizontalJump = 5 * currentFloorScale;
        linkGenerator.m_MaxVerticalFall = 15 * currentFloorScale;
        linkGenerator.m_PhysicsMask = LayerMask.GetMask("Terrain");
        linkGenerator.m_AgentRadius = agent.agentRadius;
        linkGenerator.m_AgentHeight = agent.agentHeight;
        GenerateLinks(linkGenerator);
        yield return null;

        yield return spawner.spawnLevel(wfc.generationData.spawns, currentMap.floors[currentFloorIndex].sparseness, currentMap.difficulty, currentMap.floors[currentFloorIndex].encounters);
        FindObjectsOfType<PlayerGhost>().ToList().ForEach(ghost => ghost.RpcSetCompassDirection(tileDirection));
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

        edge_list = edge_list.SelectMany((e) => e.split(gen.m_MaxHorizontalJump * 1.0f));

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
                hit = hit && (nav_hit.position.y < mid.y);
                bool is_original_edge = edge.IsPointOnEdge(nav_hit.position);
                hit &= !is_original_edge; // don't count self
                                          //~ Debug.DrawLine(phys_hit.point, nav_hit.position, hit ? navmesh_found : navmesh_missing, k_DrawDuration);
                Vector3 horizontalDist = nav_hit.position - mid;
                horizontalDist.y = 0;
                //remove drops that dont go very far
                hit &= horizontalDist.magnitude > gen.m_MaxHorizontalJump * 0.2f;
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
                    var height_delta = mid.y - nav_hit.position.y;
                    if (height_delta < gen.m_AgentHeight * 0.5f)
                    {
                        //ignore small drops
                        continue;
                    }
                    var prefab = gen.m_JumpLinkPrefab;
                    if (height_delta > gen.m_MaxVerticalJump)
                    {
                        prefab = gen.m_FallLinkPrefab;
                    }
                    var t = Instantiate(prefab).transform;
                    Debug.Assert(t != null, $"Failed to instantiate {prefab}");
                    t.SetParent(parent);
                    t.SetPositionAndRotation(mid, edge.m_Away);
                    var link = t.GetComponent<NavMeshLink>();

                    // Push endpoint out into the navmesh to ensure good
                    // connection. Necessary to prevent invalid links.
                    var inset = 0.05f;
                    link.startPoint = link.transform.InverseTransformPoint(mid - fwd * inset);
                    link.endPoint = link.transform.InverseTransformPoint(nav_hit.position) + (fwd * inset);
                    link.width = edge.m_Length;
                    link.UpdateLink();
                    //Debug.Log("Created NavLink");
                    //Undo.RegisterCompleteObjectUndo(link.gameObject, "Create NavMeshLink");


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
            return DistanceSqToPointOnLine(m_StartPos, m_EndPos, point) < 0.1f;
        }

        public IEnumerable<NavEdge> split(float maxLength)
        {
            if(m_Length < maxLength)
            {
                return new NavEdge[] { this };
            }

            int divisions = Mathf.FloorToInt(m_Length / maxLength);
            float divisionLength = m_Length / divisions;
            List<NavEdge> edges = new List<NavEdge>();
            Vector3 dir = m_EndPos - m_StartPos;
            dir.Normalize();
            Vector3 edgeVec = dir * divisionLength;

            for(int i = 0; i < divisions; i++)
            {
                NavEdge e = new NavEdge(m_StartPos+ edgeVec* i, m_StartPos+ edgeVec* (i+1));
                e.ComputeDerivedData();
                edges.Add(e);
            }
            return edges;

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

