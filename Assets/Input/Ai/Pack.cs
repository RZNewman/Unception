using Mirror;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static EventManager;

public class Pack : NetworkBehaviour
{
    List<GameObject> pack = new List<GameObject>();
    public GameObject spawnWarning;


    [HideInInspector]
    public float scale;

    SoundManager sound;

    Encounter encounter;
    Seeker seeker;
    List<Vector3> spawns;
    Vector3 rootNavPos;
    float packSpawnRadius;

    //Server
    //bool aggroed = false;
    //bool enabledUnits = false;
    private void Start()
    {
        sound = SoundManager.inst;
        
    }

    public void init()
    {
        //foreach (GameObject u in pack)
        //{
        //    u.GetComponent<EventManager>().AggroEvent += (AggroEventData agg) => {
        //        if (!agg.lostAggro)
        //        {
        //            aggroed = true;
        //        }
        //    };
        //}
        //disableUnits();
    }


    public float rewardPercent
    {
        get
        {
            return pack.Sum(u => u.GetComponent<Reward>().rewardPercent);
        }
    }

    public void setEncounter(Encounter e)
    {
        encounter = e;
        foreach(GameObject u in pack)
        {
            u.GetComponent<EventManager>().AggroEvent += (AggroEventData agg) => {
                if (agg.lostAggro)
                {
                    u.transform.position = encounter.transform.position;
                    u.GetComponentInChildren<AggroHandler>().addAggro(agg.targetCollider);
                }
                else
                {
                    encounter.trySetCombat(agg.targetCollider);
                }
                
            };
            u.GetComponent<UnitRingInd>().addColor(Color.red);
        }
        disableUnits();
    }

    List<GameObject> players = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        //if (isServer)
        //{
        //    if (other.GetComponentInParent<TeamOwnership>().getTeam() == TeamOwnership.PLAYER_TEAM)
        //    {
        //        players.Add(other.gameObject);
        //        if (!enabledUnits)
        //        {
        //            enabledUnits = true;
        //            StartCoroutine(enableUnits());
        //        }

        //    }

        //}

    }
    private void OnTriggerExit(Collider other)
    {
        //if (isServer && !aggroed)
        //{
        //    players.Remove(other.gameObject);
        //    if (players.Count == 0)
        //    {
        //        disableUnits();
        //        enabledUnits = false;
        //    }
        //}

    }



    public void addToPack(GameObject u)
    {
        pack.Add(u);
    }
    public void packDeath(GameObject u)
    {
        pack.Remove(u);
    }

    public bool packAlive()
    {
        return pack.Count > 0;
    }


    [Server]
    public void disableItemReward()
    {
        foreach (GameObject u in pack)
        {
            u.GetComponent<Reward>().givesItems = false;
        }

    }
    [Server]
    public void disableUnits()
    {
        foreach (GameObject u in pack)
        {
            u.GetComponent<EventManager>().suscribeDeath((bool _) => pack.Remove(u));
            u.SetActive(false);
        }
        RpcDisablePack(pack);
    }

    [ClientRpc]
    void RpcDisablePack(List<GameObject> packSync)
    {
        if (isClientOnly)
        {
            foreach (GameObject u in packSync)
            {
                u.SetActive(false);
            }
        }

    }

    [Server]
    IEnumerator enableUnits()
    {
        foreach (GameObject u in pack)
        {
            u.SetActive(true);
            yield return null;
        }
        RpcEnablePack(pack);
    }

    [ClientRpc]
    void RpcEnablePack(List<GameObject> packSync)
    {
        if (isClientOnly)
        {
            foreach (GameObject u in packSync)
            {
                u.SetActive(true);
            }
        }

    }
    [Server]
    public void enableUnitsChain(Vector3 location)
    {
        pack = pack.OrderBy(p => (p.transform.position - location).magnitude).Reverse().ToList();
        StartCoroutine(unitChainCoroutine());
    }

    //server
    IEnumerator unitChainCoroutine()
    {
        foreach (GameObject u in pack)
        {
            StartCoroutine(unitSpawnCoroutine(u));
            yield return new WaitForSeconds(0.3f);
        }

    }
    //server
    IEnumerator unitSpawnCoroutine(GameObject u)
    {
        GameObject o = Instantiate(spawnWarning, u.transform.position, Quaternion.identity);
        o.transform.localScale = scale * Vector3.one;
        NetworkServer.Spawn(o);
        yield return new WaitForSeconds(1f);
        Destroy(o);
        sound.sendSound(SoundManager.SoundClip.EncounterSpawn, u.transform.position);
        u.SetActive(true);
    }


    public void packAggro(GameObject target)
    {
        foreach (GameObject a in pack)
        {
            a.GetComponent<ControlManager>().spawnControl(); //spawn early so we can access aggro in encoutners
            a.GetComponentInChildren<AggroHandler>().addAggro(target);
        }
    }

    public void reposition(float radius)
    {
        seeker = GetComponent<Seeker>();
        spawns = new List<Vector3>();

        rootNavPos = AstarPath.active.GetNearest(transform.position).position;
        packSpawnRadius = radius;
        attemptPath();

    }

    int pathAttempts = 0;
    void attemptPath()
    {
        Debug.Log("Path attempt:" + pathAttempts);
        if(spawns.Count >= pack.Count || pathAttempts > 60)
        {
            spawns.Shuffle();
            for (int i = 0; i < pack.Count; i++)
            {
                pack[i].transform.position = spawns[i % spawns.Count];
            }
            return;
        }

        pathAttempts++;

        Vector2 circlePoint = Random.insideUnitCircle;
        Vector3 planePoint = transform.position + new Vector3(circlePoint.x, 0, circlePoint.y) * packSpawnRadius;

        NNInfo nodeInfo = AstarPath.active.GetNearest(planePoint);
        //if (NavMesh.SamplePosition(transform.position, out hit, sizeC.distance * 3, NavMesh.AllAreas))
        if (nodeInfo.node != null && (nodeInfo.position - transform.position).magnitude < packSpawnRadius)
        {
            seeker.StartPath(rootNavPos, nodeInfo.position,pathCallback);
        }
        else
        {
            attemptPath();
        }

    }

    void pathCallback(Path p)
    {
        if (p.CompleteState == PathCompleteState.Complete && p.vectorPath.distance() < packSpawnRadius * 1.3f)
        {
            spawns.Add(p.vectorPath[p.vectorPath.Count-1] + Vector3.up * 1 * scale);
        }
        attemptPath();

    }

    private void OnDestroy()
    {
        foreach (GameObject a in pack)
        {
            Destroy(a);
        }
    }
}
