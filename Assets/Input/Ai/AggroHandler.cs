using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventManager;
using static UnityEngine.GraphicsBuffer;

public class AggroHandler : MonoBehaviour
{
    public float aggroRadius = 15f;
    SphereCollider col;

    struct AggroData
    {
        public GameObject targetCollider;
        public GameObject targetParent;
        public float lastSeenTime;
        public float lastLookTime;
    }

    List<AggroData> aggroedEnemies = new List<AggroData>();
    List<GameObject> sensedEnemies = new List<GameObject>();
    List<GameObject> sensedAllies = new List<GameObject>();

    Combat combat;
    bool started = false;

    private void Start()
    {
        col = GetComponent<SphereCollider>();
        setCombat();
        GetComponentInParent<Power>().subscribePower(setRadius);
        transform.parent.GetComponent<EventManager>().HitEvent += aggroWhenHit;
        started = true;
    }
    void setCombat()
    {
        if (combat == null)
        {
            combat = GetComponentInParent<Combat>(true);
            combat.setAggroHandler(this);
        }
    }
    void setRadius(Power p)
    {
        col.radius = aggroRadius * p.scalePhysical();
    }
    private void OnTriggerEnter(Collider otherCol)
    {
        GameObject other = otherCol.gameObject;
        uint theirTeam = other.GetComponentInParent<TeamOwnership>().getTeam();
        uint myTeam = GetComponentInParent<TeamOwnership>().getTeam();
        if (myTeam != theirTeam)
        {
            sensedEnemies.Add(other);

        }
        else
        {
            foreach(AggroData enemy in aggroedEnemies)
            {
                other.GetComponentInParent<UnitPropsHolder>().GetComponentInChildren<AggroHandler>().addAggro(enemy.targetCollider);
            }
            sensedAllies.Add(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        sensedEnemies.Remove(other.gameObject);
        sensedAllies.Remove(other.gameObject);
    }

    static readonly float LookUpdateTime = 0.5f;
    static readonly float LooseAggroTime = 8f;
    private void Update()
    {
        List<GameObject> sensed = new List<GameObject>(sensedEnemies);
        foreach (GameObject o in sensed)
        {

            if (canSee(o))
            {
                addAggro(o);
                sensedEnemies.Remove(o);
            }
        }
        for(int i =0; i< aggroedEnemies.Count; i++)
        {
            AggroData d = aggroedEnemies[i];
            if(d.lastLookTime+LookUpdateTime < Time.time)
            {
                d.lastLookTime = Time.time;
                if (canSee(d.targetCollider))
                {
                    d.lastSeenTime = Time.time;
                }
            }

            if(d.lastSeenTime + LooseAggroTime < Time.time)
            {
                aggroedEnemies.RemoveAt(i);
                combat.dropCombat(d.targetParent);
                transform.parent.GetComponent<EventManager>().fireAggro(new AggroEventData
                {
                    lostAggro = true,
                    targetCollider = d.targetCollider
                });
                i--;
            }
            else
            {
                aggroedEnemies[i] = d;
            }

            
        }
    }


    void aggroWhenHit(GetHitEventData data)
    {
        GameObject other = data.other.GetComponentInChildren<Size>().gameObject;
        addAggro(other);
    }

    public void addAggro(GameObject target)
    {
        if (!aggroedEnemies.Select((ag) => ag.targetCollider).Contains(target))
        {
            setCombat();
            GameObject targetParent = target.GetComponentInParent<Combat>().gameObject;
            aggroedEnemies.Add(new AggroData
            {
                targetCollider = target,
                targetParent = targetParent,
                lastLookTime = Time.time,
                lastSeenTime = Time.time,
            });
            combat.setFighting(targetParent);
            transform.parent.GetComponent<EventManager>().fireAggro(new AggroEventData
            {
                lostAggro = false,
                targetCollider = target
            });
            aggroAllies(target);
        }
    }

    void aggroAllies(GameObject target)
    {
        foreach(GameObject ally in sensedAllies)
        {

            //TODO why can this be null/destroyed?
            if (ally == null)
            {
                continue;
            }
            ally.GetComponentInParent<UnitPropsHolder>(true).GetComponentInChildren<AggroHandler>(true).addAggro(target);
        }
    }
    
    public void removeTarget(GameObject target)
    {
        aggroedEnemies.Remove(aggroedEnemies.Find((ag) =>ag.targetCollider == target));
    }

    public GameObject getTopTarget()
    {
        if (started && aggroedEnemies.Count > 0)
        {
            return aggroedEnemies[0].targetCollider;
        }
        return null;
    }
    public bool canSee(GameObject other)
    {
        Vector3 diff = other.transform.position - transform.position;
        return !Physics.Raycast(transform.position, diff, diff.magnitude, LayerMask.GetMask("Terrain"));
    }
}
