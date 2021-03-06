using System.Collections.Generic;
using UnityEngine;

public class AggroHandler : MonoBehaviour
{
    public float aggroRadius = 15f;
    SphereCollider col;

    [HideInInspector]
    public List<GameObject> aggro;

    Pack pack;

    private void Start()
    {
        aggro = new List<GameObject>();
        col = GetComponent<SphereCollider>();

        pack = transform.parent.GetComponentInChildren<PackTag>().owner;
        if (pack)
        {
            pack.addToPack(this);
        }
        GetComponentInParent<Power>().subscribePower(setRadius);

    }
    void setRadius(Power p)
    {
        col.radius = aggroRadius * p.scale();
    }
    private void OnTriggerEnter(Collider other)
    {
        GameObject them = other.gameObject;
        uint theirTeam = them.GetComponentInParent<TeamOwnership>().getTeam();
        uint myTeam = GetComponentInParent<TeamOwnership>().getTeam();
        if (myTeam != theirTeam)
        {
            if (pack)
            {
                if (!aggro.Contains(them))
                {
                    pack.packAggro(them);
                }
            }
            else
            {
                addAggro(them);
            }


        }
    }
    public void addAggro(GameObject target)
    {
        if (!aggro.Contains(target))
        {
            aggro.Add(target);
            GetComponentInParent<Combat>().aggro(target.GetComponentInParent<Combat>().gameObject);
        }
    }

    public GameObject getTopTarget()
    {
        if (aggro.Count > 0)
        {
            return aggro[0];
        }
        return null;
    }
}
