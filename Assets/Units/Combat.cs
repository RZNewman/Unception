using Mirror;
using UnityEngine;

public class Combat : NetworkBehaviour
{
    public readonly SyncList<GameObject> active = new SyncList<GameObject>();

    GameObject lastUnitHitBy;
    private void Start()
    {
        GetComponent<LifeManager>().suscribeDeath(onDeath);
    }
    public void aggro(GameObject other)
    {
        if (!active.Contains(other))
        {
            active.Add(other);
            other.GetComponent<Combat>().addTarget(gameObject);
            //TODO Aggro handler adds a callback here, to aggro the pack when hit
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
        clearAggro();
    }
    void rewardKiller()
    {
        if (lastUnitHitBy)
        {
            lastUnitHitBy.GetComponent<Power>().absorb(GetComponent<Power>());
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
        if (lastUnitHitBy && lastUnitHitBy == other)
        {
            lastUnitHitBy = null;
        }
    }
    public void getHit(GameObject other)
    {
        lastUnitHitBy = other;
        aggro(other);
    }

    public bool inCombat
    {
        get
        {
            return active.Count > 0;
        }
    }
}
