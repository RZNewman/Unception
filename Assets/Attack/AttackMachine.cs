using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;

public class AttackMachine
{
    List<AttackSegment> segments;
    Ability ability;
    UnitMovement mover;
    bool hardCast;

    bool init = false;
    bool ended = false;
    public bool hasFinished
    {
        get
        {
            return ended;
        }
    }
    StateMachine<AttackStageState> attackMachine;
    AttackSegment currentSegment;
    public AttackMachine(Ability a, UnitMovement m, bool hardCasted)
    {
        hardCast = hardCasted;
        ability = a;
        mover = m;
        segments = ability.cast(mover, hardCast);
        attackMachine = new StateMachine<AttackStageState>(getNextState);
        init = true;

    }
    public void tick()
    {
        attackMachine.tick();
    }
    public void transition()
    {
        attackMachine.transition();
        currentSegment.sourceUpdate();
    }
    public void exit()
    {
        ability.startCooldown();
        attackMachine.exit();
        currentSegment.exitSegment();
    }

    public void indicatorUpdate()
    {
        currentSegment.IndicatorUpdate();
    }
    public float remainingWindDown
    {
        get
        {
            return currentSegment.remainingWindDown();
        }
    }
    public bool inWindup
    {
        get
        {
            return currentSegment.inWindup();
        }
    }
    public AttackSegment segment
    {
        get
        {
            return currentSegment;
        }
    }
    AttackStageState getNextState()
    {
        AttackStageState s = null;
        System.Action nextSegment = () =>
        {
            if (hardCast)
            {
                mover.maybeSnapRotation(mover.input);
            }       
            currentSegment = segments[0];
            s = currentSegment.enterSegment(mover);
        };
        if (!init)
        {
            nextSegment();
        }
        else
        {
            s = currentSegment.getNextState();
            if (s == null)
            {
                segments.RemoveAt(0);
                if (segments.Count == 0)
                {
                    ended = true;
                    return new WindState(mover);
                }
                currentSegment.exitSegment();
                nextSegment();
            }
        }

        return s;
    }
}
