using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackSegment;
using static AttackUtils;
using static GenerateHit;
using static Utils;

public class AttackMachine
{
    List<AttackSegment> segments;
    Ability ability;
    UnitMovement mover;
    bool hardCast;

    bool init = false;
    bool ended = false;
    bool didExit = false;
    public bool hasFinished
    {
        get
        {
            return ended;
        }
    }

    StateMachine<AttackStageState> attackMachine;
    AttackSegment currentSegment;
    public struct CastingLocationData
    {
        public bool hardCast;
        public SourceLocation locationOverride;
        public Vector3 triggeredPosition;

        public bool usesRangeForHitbox(HitType typeOfHit)
        {
            Func<HitType, bool> isAttached = (type) => {
                return type == HitType.Attached;
            };

            return locationOverride switch
            {
                SourceLocation.Body => isAttached(typeOfHit),
                SourceLocation.BodyFixed => isAttached(typeOfHit),
                _ => false,
            };
        }
    }

    public delegate void MachineEndCallback(AttackMachine m);
    MachineEndCallback callback;


    public AttackMachine(Ability a, UnitMovement m, CastingLocationData castData, MachineEndCallback cb = null)
    {
        hardCast = castData.hardCast;
        ability = a;
        mover = m;
        segments = ability.cast(mover, castData);
        attackMachine = new StateMachine<AttackStageState>(getNextState);
        init = true;
        callback = cb;
    }
    public void tick()
    {
        attackMachine.tick();
    }
    public void transition()
    {
        currentSegment.sourcePreUpdate();
        attackMachine.transition();
        if (ended)
        {
            exit();
        }

    }
    public void exit()
    {
        if (!didExit)
        {
            ability.startCooldown();
            attackMachine.exit();
            currentSegment.exitSegment();
            didExit = true;
            if (callback != null)
            {
                callback(this);
            }
        }

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
            StateTransitionInfo info = currentSegment.getNextState();
            s = info.state;
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
            if (info.triggeredCast)
            {
                ability.GetComponentInParent<EventManager>().fireCast(ability);
            }
        }

        return s;
    }
}
