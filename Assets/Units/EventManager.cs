using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void OnDeath(bool natural);
    public delegate void OnHit(GameObject other);
    public delegate void OnCast(Ability ability);
    public delegate void OnTransition();
    public delegate void OnTick();
    public delegate void OnIndicator();



    event OnDeath DeathEvent;
    public event OnHit HitEvent;
    public event OnCast CastEvent;
    public event OnTransition TransitionEvent;
    public event OnTick TickEvent;
    public event OnIndicator IndicatorEvent;

    bool deathFired = false;

    public void suscribeDeath(OnDeath d)
    {
        DeathEvent += d;
    }
    public void fireDeath(bool natural)
    {
        DeathEvent?.Invoke(natural);
        deathFired = true;
    }

    private void OnDestroy()
    {
        if (!deathFired)
        {
            fireDeath(false);
        }

    }


    public void fireHit(GameObject other)
    {
        HitEvent?.Invoke(other);
    }


    public void fireCast(Ability a)
    {
        CastEvent?.Invoke(a);
    }

    public void fireTransition()
    {
        TransitionEvent?.Invoke();
    }

    public void fireTick()
    {
        TickEvent?.Invoke();
    }

    public void fireIndicator()
    {
        IndicatorEvent?.Invoke();
    }
}
