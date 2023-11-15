using Mirror;
using UnityEngine;
using static EventManager;

public class Combat : NetworkBehaviour
{
    public readonly SyncList<GameObject> active = new SyncList<GameObject>();

    GameObject lastUnitHitBy;

    AggroHandler aggro;

    private void Start()
    {
        if (!inCombat)
        {
            setUnitUI(false, false);
        }      
        if (isServer)
        {
            EventManager events = GetComponent<EventManager>();
            events.suscribeDeath(onDeath);
            events.HitEvent += (onHit);
        }

    }
    void setUnitUI(bool active, bool aiOnly = true)
    {
        LocalPlayer player = GetComponent<LocalPlayer>();
        //Encounters dont have UI or LocalPlayer
        if (player)
        {
            if (!player.isLocalUnit || !aiOnly)
            {
                gameObject.GetComponentInChildren<UnitUiReference>(true).gameObject.SetActive(active);
            }
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
            setUnitUI(true);
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
            PackHeal heal = GetComponent<PackHeal>();
            if (heal)
            {
                lastUnitHitBy.GetComponent<PackHeal>().rewardKill(heal.percentHealKiller);
            }

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

    public void dropCombat(GameObject other)
    {
        if (active.Contains(other))
        {
            removeTarget(other);
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
        if (!inCombat)
        {
            setUnitUI(false);
        }
        if (aggro)
        {
            aggro.removeTarget(other.GetComponentInChildren<Size>().gameObject);
        }
    }
    void onHit(GetHitEventData data)
    {
        lastUnitHitBy = data.other;
        setFighting(data.other);
    }

    public void setHitBy(GameObject hitter)
    {
        lastUnitHitBy = hitter;
    }
    public bool inCombat
    {
        get
        {
            return active.Count > 0;
        }
    }
}
