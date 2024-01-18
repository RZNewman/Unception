using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static TeamOwnership;

public class Encounter : NetworkBehaviour
{
    public GameObject activeInd;

    [HideInInspector]
    public GameObject revealOnEnd;


    List<Pack> packs = new List<Pack>();
    float scale;
    Combat combat;

    bool encounterRunning = false;
    GameObject triggeringUnit;
    GameObject triggeringUnitBody;
    Pack currentPack = null;
    //Server
    public void addPack(Pack p)
    {
        packs.Add(p);
        p.setEncounter(this);
    }
    public void setScale(float s)
    {
        scale = s;
    }

    public float rewardPercent
    {
        get
        {
            return packs.Sum(p => p.rewardPercent);
        }
    }

    private void OnTriggerEnter(Collider otherCol)
    {
        if (!isServer)
        {
            return;
        }
        GameObject colliderBody = otherCol.gameObject;
        uint theirTeam = colliderBody.GetComponentInParent<TeamOwnership>().getTeam();
        if (ENEMY_TEAM != theirTeam && !encounterRunning)
        {
            triggeringUnitBody = colliderBody;
            triggeringUnit = triggeringUnitBody.GetComponentInParent<Combat>().gameObject;
            combat.setFighting(triggeringUnit);
            encounterRunning = true;
            launchPack();
            RpcClientActivate();
        }
    }

    public void trySetCombat(GameObject colliderBody) {
        if (!combat.inCombat)
        {
            triggeringUnitBody = colliderBody;
            triggeringUnit = triggeringUnitBody.GetComponentInParent<Combat>().gameObject;
            combat.setFighting(triggeringUnit);
        }
    }

    [ClientRpc]
    void RpcClientActivate()
    {
        activeInd.SetActive(true);
        FindObjectOfType<SoundManager>().playSound(SoundManager.SoundClip.EncounterStart, transform.position, 3f);
    }

    private void Update()
    {
        if (encounterRunning)
        {

            if (!currentPack.packAlive())
            {
                packs.RemoveAt(0);
                if (packs.Count == 0)
                {
                    encounterRunning = false;
                    combat.setHitBy(triggeringUnit);
                    if (revealOnEnd)
                    {
                        revealOnEnd.SetActive(true);
                    }
                    GetComponent<LifeManager>().die();
                }
                else
                {
                    launchPack();
                }

            }
        }
    }
    void launchPack()
    {
        currentPack = packs[0];
        currentPack.packAggro(triggeringUnitBody);
        currentPack.enableUnitsChain(triggeringUnit.transform.position);

    }
    private void OnDestroy()
    {
        foreach (Pack p in packs)
        {
            Destroy(p.gameObject);
        }
    }


    readonly float ambushRadius = 30;
    private void Start()
    {
        


        
    }
    public void init()
    {
        combat = GetComponent<Combat>();
        float scaledAmbushRadius = scale * ambushRadius;
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();

        Vector3 rootPos;
        if (NavMesh.SamplePosition(transform.position, out hit, scaledAmbushRadius, NavMesh.AllAreas))
        {
            rootPos = hit.position;
        }
        else
        {
            rootPos = transform.position;
        }
        //server
        foreach (Pack p in packs)
        {
            p.disableUnits();
            p.disableItemReward();
            List<Vector3> spawns = new List<Vector3>();

            for (int i = 0; i < 60 && spawns.Count < p.unitCount; i++)
            {
                Vector2 circlePoint = Random.insideUnitCircle;
                Vector3 planePoint = transform.position + new Vector3(circlePoint.x, 0, circlePoint.y) * scaledAmbushRadius;

                if (NavMesh.SamplePosition(planePoint, out hit, scaledAmbushRadius, NavMesh.AllAreas))
                {
                    Vector3 target = hit.position;
                    NavMesh.CalculatePath(rootPos, target, NavMesh.AllAreas, path);
                    if (path.status == NavMeshPathStatus.PathComplete && path.distance() < scaledAmbushRadius * 1.3f)
                    {
                        spawns.Add(target + Vector3.up * 1 * scale);
                    }
                    else
                    {
                        i--;
                    }

                }
            }



            p.reposition(spawns);
        }
    }
}
