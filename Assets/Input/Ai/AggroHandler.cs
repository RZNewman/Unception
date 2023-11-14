using System.Collections.Generic;
using UnityEngine;
using static EventManager;
using static UnityEngine.GraphicsBuffer;

public class AggroHandler : MonoBehaviour
{
    public float aggroRadius = 15f;
    SphereCollider col;


    List<GameObject> aggroedEnemies = new List<GameObject>();
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
            foreach(GameObject enemy in aggroedEnemies)
            {
                other.GetComponentInParent<UnitPropsHolder>().GetComponentInChildren<AggroHandler>().addAggro(enemy);
            }
            sensedAllies.Add(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        sensedEnemies.Remove(other.gameObject);
        sensedAllies.Remove(other.gameObject);
    }
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
    }


    void aggroWhenHit(GetHitEventData data)
    {
        GameObject other = data.other.GetComponentInChildren<Size>().gameObject;
        addAggro(other);
    }

    public void addAggro(GameObject target)
    {
        if (!aggroedEnemies.Contains(target))
        {
            setCombat();
            aggroedEnemies.Add(target);
            combat.setFighting(target.GetComponentInParent<Combat>().gameObject);
            transform.parent.GetComponent<EventManager>().fireAggro(target);
            aggroAllies(target);
        }
    }

    void aggroAllies(GameObject target)
    {
        foreach(GameObject ally in sensedAllies)
        {
            ally.GetComponentInParent<UnitPropsHolder>().GetComponentInChildren<AggroHandler>().addAggro(target);
        }
    }
    
    public void removeTarget(GameObject target)
    {
        aggroedEnemies.Remove(target);
    }

    public GameObject getTopTarget()
    {
        if (started && aggroedEnemies.Count > 0)
        {
            return aggroedEnemies[0];
        }
        return null;
    }
    public bool canSee(GameObject other)
    {
        Vector3 diff = other.transform.position - transform.position;
        return !Physics.Raycast(transform.position, diff, diff.magnitude, LayerMask.GetMask("Terrain"));
    }
}
