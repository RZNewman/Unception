using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void OnDeath(bool natural);
    public delegate void OnHit(GameObject other);
    public delegate void OnCast(Ability ability);
    public delegate void OnTransition();
    public delegate void OnTick();
    public delegate void OnIndicator();



    List<OnDeath> OnDeathCallbacks = new List<OnDeath>();
    List<OnHit> OnHitCallbacks = new List<OnHit>();
    List<OnCast> OnCastCallbacks = new List<OnCast>();
    List<OnTransition> OnTransitionCallbacks = new List<OnTransition>();
    List<OnTick> OnTickCallbacks = new List<OnTick>();
    List<OnIndicator> OnIndicatorCallbacks = new List<OnIndicator>();

    bool deathFired = false;

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
        deathFired = true;
    }

    private void OnDestroy()
    {
        if (!deathFired)
        {
            fireDeath(false);
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

    public void subscribeCast(OnCast c)
    {
        OnCastCallbacks.Add(c);
    }
    public void fireCast(Ability a)
    {
        foreach (OnCast c in OnCastCallbacks)
        {
            c(a);
        }
    }


    public void subscribeTransition(OnTransition t)
    {
        OnTransitionCallbacks.Add(t);
    }
    public void unsubscribeTransition(OnTransition t)
    {
        OnTransitionCallbacks.Remove(t);
    }

    public void fireTransition()
    {
        List<OnTransition> transitions = new List<OnTransition>(OnTransitionCallbacks);
        foreach (OnTransition c in transitions)
        {
            c();
        }
    }
    public void subscribeTick(OnTick t)
    {
        OnTickCallbacks.Add(t);
    }
    public void unsubscribeTick(OnTick t)
    {
        OnTickCallbacks.Remove(t);
    }

    public void fireTick()
    {
        List<OnTick> ticks = new List<OnTick>(OnTickCallbacks);
        foreach (OnTick c in ticks)
        {
            c();
        }
    }
    public void subscribeIndicator(OnIndicator t)
    {
        OnIndicatorCallbacks.Add(t);
    }
    public void unsubscribeIndicator(OnIndicator t)
    {
        OnIndicatorCallbacks.Remove(t);
    }

    public void fireIndicator()
    {
        foreach (OnIndicator c in OnIndicatorCallbacks)
        {
            c();
        }
    }
}
