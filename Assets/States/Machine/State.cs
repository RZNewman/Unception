using UnityEngine;

public abstract class State
{
    public struct StateTransition
    {
        public State nextState;
        public bool shouldTrasition;
        public bool expired;
        public StateTransition(State s, bool t, bool e = false)
        {
            this.nextState = s;
            this.shouldTrasition = t;
            this.expired = e;
        }
    }
    public enum DurrationType
    {
        None,
        Timed
    }
    DurrationType durationType;
    float duration;
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

    public virtual StateTransition transition()
    {
        if (durationType == DurrationType.Timed && duration <= 0)
        {
            return new StateTransition(null, true, true);
        }

        return new StateTransition(null, false);
    }
    public virtual void exit(bool expired) { }

}
