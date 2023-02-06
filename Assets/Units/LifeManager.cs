using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : NetworkBehaviour
{
    GameObject unitBody
    {
        get
        {
            return GetComponentInChildren<Size>().gameObject;
        }
    }

    public delegate void OnDeath(bool natural);
    public delegate void OnHit(GameObject other);

    List<OnDeath> OnDeathCallbacks = new List<OnDeath>();
    List<OnHit> OnHitCallbacks = new List<OnHit>();

    private void Start()
    {
    }

    bool isDead = false;

    public bool IsDead
    {
        get { return isDead; }
    }

    public void suscribeDeath(OnDeath d)
    {
        OnDeathCallbacks.Add(d);
    }

    public void suscribeHit(OnHit h)
    {
        OnHitCallbacks.Add(h);
    }

    public void getHit(GameObject other)
    {
        foreach (OnHit c in OnHitCallbacks)
        {
            c(other);
        }
    }

    public void die()
    {
        isDead = true;
        foreach (OnDeath c in OnDeathCallbacks)
        {
            c(true);
        }

        Destroy(gameObject);


    }
    private void OnDestroy()
    {
        if (!isDead)
        {
            foreach (OnDeath c in OnDeathCallbacks)
            {
                c(false);
            }
        }

    }

}
