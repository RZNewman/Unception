using Mirror;
using UnityEngine;

public class Combat : NetworkBehaviour
{
    public readonly SyncList<GameObject> active = new SyncList<GameObject>();

    GameObject lastUnitHitBy;



    private void Start()
    {
        if (isServer)
        {
            LifeManager life = GetComponent<LifeManager>();
            life.suscribeDeath(onDeath);
            life.suscribeHit(onHit);
        }

    }
    public void setFighting(GameObject other)
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

    void onDeath()
    {
        rewardKiller();
        clearFighting();
    }
    void rewardKiller()
    {
        if (lastUnitHitBy)
        {
            lastUnitHitBy.GetComponent<Reward>().recieveReward(GetComponent<Reward>());
        }
    }
    void clearFighting()
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
        if (lastUnitHitBy && lastUnitHitBy == other)
        {
            lastUnitHitBy = null;
        }
    }
    void onHit(GameObject other)
    {
        lastUnitHitBy = other;
        setFighting(other);
    }

    public bool inCombat
    {
        get
        {
            return active.Count > 0;
        }
    }
}
