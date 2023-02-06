using Mirror;
using UnityEngine;

public class Combat : NetworkBehaviour
{
    public readonly SyncList<GameObject> active = new SyncList<GameObject>();

    GameObject lastUnitHitBy;

    AggroHandler aggro;

    private void Start()
    {
        if (isServer)
        {
            LifeManager life = GetComponent<LifeManager>();
            life.suscribeDeath(onDeath);
            life.suscribeHit(onHit);
        }

    }

    public void setAggroHandler(AggroHandler a)
    {
        aggro = a;
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

    void onDeath(bool natural)
    {
        rewardKiller();
        clearFighting();
    }
    void rewardKiller()
    {
        if (lastUnitHitBy)
        {
            lastUnitHitBy.GetComponent<Reward>().recieveReward(GetComponent<Reward>());
            lastUnitHitBy.GetComponent<PackHeal>().rewardKill(GetComponent<PackHeal>().packPool);
        }
    }
    void clearFighting()
    {
        foreach (GameObject other in active)
        {

            if (other)
            {
                other.GetComponent<Combat>().removeTarget(gameObject);
            }

        }
        active.Clear();
    }
    void removeTarget(GameObject other)
    {
        active.Remove(other);
        if (lastUnitHitBy && lastUnitHitBy == other)
        {
            lastUnitHitBy = null;
        }
        if (aggro)
        {
            aggro.removeTarget(other.GetComponentInChildren<Size>().gameObject);
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
