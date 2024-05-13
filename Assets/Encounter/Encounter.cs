using Mirror;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst.CompilerServices;
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
                        revealOnEnd.GetComponent<Interaction>().setInteractable(true);
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

        
        //server
        foreach (Pack p in packs)
        {
            p.disableUnits();
            p.disableItemReward();
            p.reposition(scaledAmbushRadius);
        }
    }

}
