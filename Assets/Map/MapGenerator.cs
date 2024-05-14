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
using UnityEditor;
using NavmeshLinksGenerator;
using Pathfinding;
using static FloorNormal;

public class MapGenerator : NetworkBehaviour
{
    public GameObject fallPre;
    public GameObject jumpPre;
    public GameObject floorRootPre;
    public GameObject endPortalPre;

    public List<WeightedProp> props;
    [System.Serializable]
    public struct WeightedProp
    {
        public int weight;
        public GameObject prop;
    }


    float currentFloorScale = 1f;
    GameObject currentFloor;
    Atlas atlas;
    AstarData navData;
    RecastGraph recastGraph;
    NavMeshLinks_AutoPlacer linkGenerator;

    MonsterSpawn spawner;
    SoundManager sound;
    WFCGeneration wfc;
    // Start is called before the first frame update
    void Start()
    {
        atlas = FindObjectOfType<Atlas>();
        sound = FindObjectOfType<SoundManager>();
        wfc = GetComponent<WFCGeneration>();
        linkGenerator = GetComponent<NavMeshLinks_AutoPlacer>();
        navData = AstarPath.active.data;
        if (isServer)
        {
            recastGraph = navData.AddGraph(typeof(RecastGraph)) as RecastGraph;
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

    public void destroyFloor()
    {
        Destroy(currentFloor);
    }
    public IEnumerator buildMap()
    {
        Destroy(currentFloor);
        //currentFloorScale = Power.scalePhysical(currentMap.power);
        currentFloorScale = 1; // server scale is set to the map, so this should always be 1
        spawner.setSpawnPower(currentMap.power, new GenerateAttack.Scales
        {
            numeric =1,
            world =1,
            time = Power.currentBaseScales.time,
        });
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
        currentFloor = Instantiate(floorRootPre, transform.position, Quaternion.identity, transform);
        spawner.setFloor(currentFloor.transform);
        currentFloor.GetComponent<ClientAdoption>().parent = gameObject;
        currentFloor.transform.localScale = Vector3.one * currentFloorScale;
        NetworkServer.Spawn(currentFloor);


        GameObject wfcFloor = Instantiate(floorRootPre, transform.position, Quaternion.identity, currentFloor.transform);
        wfcFloor.GetComponent<ClientAdoption>().parent = currentFloor;
        NetworkServer.Spawn(wfcFloor);
        yield return wfc.generate(wfcFloor, currentMap.floor.wfcParams);

        

        GameObject endPortal = Instantiate(endPortalPre, wfc.generationData.end, Quaternion.identity, currentFloor.transform);



        //if (recastGraph.isScanned)
        //{
        //    //recastGraph.
        //}
        //NavMeshBuilder.CollectSources(currentFloor.transform, LayerMask.GetMask("Terrain"), NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        //foreach (GameObject tile in wfc.generationData.navTiles)
        //{
        //    List<NavMeshBuildSource> sourcesTile = new List<NavMeshBuildSource>();
        //    NavMeshBuilder.CollectSources(tile.transform, LayerMask.GetMask("Terrain"), NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sourcesTile);
        //    sources.AddRange(sourcesTile);
        //}
        //navData = NavMesh.AddNavMeshData(NavMeshBuilder.BuildNavMeshData(agent, sources, new Bounds(Vector3.zero, Vector3.one * 4000), Vector3.zero, Quaternion.identity));
        float agentRadius = 0.6f * currentFloorScale;
        recastGraph.characterRadius = agentRadius;
        recastGraph.cellSize = agentRadius * 0.5f;
        recastGraph.walkableClimb = 0.3f * currentFloorScale;
        recastGraph.walkableHeight = WFCGeneration.tileScale.y * 1.2f * currentFloorScale;
        recastGraph.maxEdgeLength = WFCGeneration.tileScale.x * 1.0f * currentFloorScale;
        recastGraph.collectionSettings.layerMask = LayerMask.GetMask("Terrain");
        recastGraph.maxSlope = floorDegrees;
        recastGraph.forcedBoundsCenter = wfc.generationData.size / 2f;
        recastGraph.forcedBoundsSize = wfc.generationData.size;
        
        
        linkGenerator.tileWidth = WFCGeneration.tileScale.x * 0.3f * currentFloorScale;
        linkGenerator.maxJumpUpHeight = 4 * currentFloorScale;
        linkGenerator.maxJumpDist = 5 * currentFloorScale;
        linkGenerator.maxJumpDownHeight = 15 * currentFloorScale;
        linkGenerator.raycastLayerMask = LayerMask.GetMask("Terrain");
        linkGenerator.agentRadius = agentRadius;
        //linkGenerator.a = agent.agentHeight;

        recastGraph.Scan();
        Debug.Log("Nav mesh for props: " + Time.time);
        yield return null;
        List<GraphNode> nodes = new List<GraphNode>();
        recastGraph.GetNodes(nodes.Add);


        yield return spawnProps(nodes);
        yield return null;


        recastGraph.Scan();
        Debug.Log("Nav mesh for agents: " + Time.time);
        linkGenerator.Generate();
        Debug.Log("Nav links: " + Time.time);
        //yield return GenerateLinks(linkGenerator);
        yield return null;
        recastGraph.GetNodes(nodes.Add);

        

        StaticBatchingUtility.Combine(currentFloor);


        yield return spawner.spawnLevel(nodes, currentMap, endPortal,wfc.generationData.start);
        FindObjectsOfType<PlayerGhost>().ToList().ForEach(ghost => ghost.RpcSetCompassTarget(wfc.generationData.end));
    }

    IEnumerator spawnProps(List<GraphNode> nodes)
    {
        int propCount = 0;
        foreach(Vector3 location in nodes.RandomLocations(5, Atlas.baseSparseness * 0.55f))
        {
            Instantiate(props.RandomItemWeighted(n => n.weight).prop, location, Quaternion.Euler(0, Random.value, 0), currentFloor.transform);
            propCount++;
            if(propCount == 5)
            {
                propCount = 0;
                yield return null;
            }
            
        }
    }




}

