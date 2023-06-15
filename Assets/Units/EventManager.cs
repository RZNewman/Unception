using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void OnDeath(bool natural);
    public delegate void OnHit(GameObject other);

    List<OnDeath> OnDeathCallbacks = new List<OnDeath>();
    List<OnHit> OnHitCallbacks = new List<OnHit>();

    public void suscribeDeath(OnDeath d)
    {
        OnDeathCallbacks.Add(d);
    }
    public void fireDeath(bool natural)
    {
        foreach (OnDeath d in OnDeathCallbacks)
        {
            d(natural);
        }
    }

    public void suscribeHit(OnHit h)
    {
        OnHitCallbacks.Add(h);
    }

    public void fireHit(GameObject other)
    {
        foreach (OnHit c in OnHitCallbacks)
        {
            c(other);
        }
    }
}
