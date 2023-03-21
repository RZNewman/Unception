using UnityEngine;

public abstract class State
{
    public struct StateTransition
    {
        public State nextState;
        public bool shouldTransition;
        public bool expired;
        public StateTransition(State s, bool t, bool e = false)
        {
            this.nextState = s;
            this.shouldTransition = t;
            this.expired = e;
        }
    }
    public enum DurrationType
    {
        None,
        Timed
    }
    DurrationType durationType;
    float duration = 0;
    float startingDuration;

    protected float currentDurration
    {
        get
        {
            return duration;
        }
    }
    protected float maxDuration
    {
        get
        {
            return startingDuration;
        }
    }

    public State()
    {
        durationType = DurrationType.None;
    }
    public State(float t)
    {
        durationType = DurrationType.Timed;
        duration = t;
        startingDuration = t;
    }

    public virtual void enter() { }
    public virtual void tick()
    {
        if (durationType == DurrationType.Timed)
        {
            duration -= Time.fixedDeltaTime;
        }
    }
    protected bool expired
    {
        get
        {
            return durationType == DurrationType.Timed && duration <= 0;
        }
    }
    public virtual StateTransition transition()
    {
        if (expired)
        {
            return new StateTransition(null, true, true);
        }

        return new StateTransition(null, false);
    }
    public virtual void exit(bool expired) { }

}
