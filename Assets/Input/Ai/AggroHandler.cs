using System.Collections.Generic;
using UnityEngine;

public class AggroHandler : MonoBehaviour
{
    public float aggroRadius = 15f;
    SphereCollider col;

    [HideInInspector]
    public List<GameObject> aggroList;

    Pack pack;

    private void Start()
    {
        aggroList = new List<GameObject>();
        col = GetComponent<SphereCollider>();

        pack = transform.parent.GetComponent<UnitPropsHolder>().pack;
        if (pack)
        {
            pack.addToPack(this);
        }
        GetComponentInParent<Power>().subscribePower(setRadius);
        transform.parent.GetComponent<LifeManager>().suscribeHit(aggroUnitParent);

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
            aggro(other);


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
        if (aggroList.Count > 0)
        {
            return aggroList[0];
        }
        return null;
    }
}
