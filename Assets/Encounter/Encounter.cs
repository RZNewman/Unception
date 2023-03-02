using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static TeamOwnership;

public class Encounter : MonoBehaviour
{
    List<Pack>packs = new List<Pack> ();
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
    }
    public void setScale(float s)
    {
        scale =s;
    }
    private void OnTriggerEnter(Collider otherCol)
    {
        GameObject colliderBody = otherCol.gameObject;
        uint theirTeam = colliderBody.GetComponentInParent<TeamOwnership>().getTeam();
        if (ENEMY_TEAM != theirTeam && !encounterRunning)
        {
            triggeringUnitBody = colliderBody;
            triggeringUnit = triggeringUnitBody.GetComponentInParent<Combat>().gameObject;
            combat.setFighting(triggeringUnit);
            encounterRunning = true;
            launchPack();
        }
    }

    private void Update()
    {
        if (encounterRunning)
        {
            if (!combat.inCombat)
            {
                Destroy(gameObject);
            }
            
            if (!currentPack.packAlive())
            {
                packs.RemoveAt(0);
                if(packs.Count == 0)
                {
                    encounterRunning = false;
                    combat.setHitBy(triggeringUnit);
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
        currentPack.enableUnits();
        currentPack.packAggro(triggeringUnitBody);
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
        combat = GetComponent<Combat>();



        float scaledAmbushRadius = scale * ambushRadius;
        //server
        foreach (Pack p in packs)
        {
            p.disableUnits();
            p.disableItemReward();
            List<Vector3> spawns = new List<Vector3>();

            for(int i = 0; i < 60 && spawns.Count <p.unitCount; i++)
            {
                Vector2 circlePoint = Random.insideUnitCircle;
                Vector3 planePoint = transform.position + new Vector3(circlePoint.x, 0, circlePoint.y) * scaledAmbushRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(planePoint, out hit, scaledAmbushRadius, NavMesh.AllAreas))
                {
                    spawns.Add(hit.position + Vector3.up * 1 * scale);
                }
            }
            


            p.reposition(spawns);
        }
    }
}
