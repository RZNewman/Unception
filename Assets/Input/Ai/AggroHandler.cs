using System.Collections.Generic;
using UnityEngine;

public class AggroHandler : MonoBehaviour
{
    public float aggroRadius = 15f;
    SphereCollider col;


    List<GameObject> aggroList;
    List<GameObject> senseRadius;

    Pack pack;
    bool started = false;

    private void Start()
    {
        aggroList = new List<GameObject>();
        senseRadius = new List<GameObject>();
        col = GetComponent<SphereCollider>();

        pack = transform.parent.GetComponent<UnitPropsHolder>().pack;
        if (pack)
        {
            pack.addToPack(this);
        }
        GetComponentInParent<Power>().subscribePower(setRadius);
        transform.parent.GetComponent<LifeManager>().suscribeHit(aggroUnitParent);
        started = true;
    }
    void setRadius(Power p)
    {
        col.radius = aggroRadius * p.scale();
    }
    private void OnTriggerEnter(Collider otherCol)
    {
        GameObject other = otherCol.gameObject;
        uint theirTeam = other.GetComponentInParent<TeamOwnership>().getTeam();
        uint myTeam = GetComponentInParent<TeamOwnership>().getTeam();
        if (myTeam != theirTeam)
        {
            senseRadius.Add(other);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        senseRadius.Remove(other.gameObject);
    }
    private void Update()
    {
        List<GameObject> sensed = new List<GameObject>(senseRadius);
        foreach (GameObject o in sensed)
        {

            if (canSee(o))
            {
                aggro(o);
                senseRadius.Remove(o);
            }
        }
    }


    void aggroUnitParent(GameObject parent)
    {
        GameObject other = parent.GetComponentInChildren<Size>().gameObject;
        aggro(other);
    }
    public void aggro(GameObject other)
    {
        if (pack)
        {
            if (!aggroList.Contains(other))
            {
                pack.packAggro(other);
            }
        }
        else
        {
            addAggro(other);
        }
    }
    public void addAggro(GameObject target)
    {
        if (!aggroList.Contains(target))
        {
            aggroList.Add(target);
            GetComponentInParent<Combat>().setFighting(target.GetComponentInParent<Combat>().gameObject);
        }
    }

    public GameObject getTopTarget()
    {
        if (started && aggroList.Count > 0)
        {
            return aggroList[0];
        }
        return null;
    }
    public bool canSee(GameObject other)
    {
        Vector3 diff = other.transform.position - transform.position;
        return !Physics.Raycast(transform.position, diff, diff.magnitude, LayerMask.GetMask("Terrain"));
    }
}
