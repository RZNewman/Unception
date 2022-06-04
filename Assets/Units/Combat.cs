using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : NetworkBehaviour
{
    public readonly SyncList<GameObject> active = new SyncList<GameObject>();
    private void Start()
    {
        GetComponent<LifeManager>().suscribeDeath(clearAggro);
    }
    public void aggro(GameObject other)
    {
        if (!active.Contains(other))
        {
            active.Add(other);
            other.GetComponent<Combat>().addTarget(gameObject);
        }
    }


    void addTarget(GameObject other)
    {
        if (!active.Contains(other))
        {
            active.Add(other);
        }
    }

    void clearAggro()
    {
        foreach (GameObject other in active)
        {
            active.Remove(other);
            other.GetComponent<Combat>().removeTarget(gameObject);
        }
    }
    void removeTarget(GameObject other)
    {
        active.Remove(other);
    }

    public bool inCombat
    {
        get
        {
            return active.Count > 0;
        }
    }
}
