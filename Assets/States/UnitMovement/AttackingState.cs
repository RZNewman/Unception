using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static UnitControl;

public class AttackingState : PlayerMovementState
{
    Ability castingAbility;

    StateMachine<AttackStageState> attackMachine;
    bool ended = false;

    List<AttackSegment> segments;
    AttackSegment currentSegment;


    public AttackingState(UnitMovement m, Ability atk) : base(m)
    {
        castingAbility = atk;
    }

    public string abilityName
    {
        get
        {
            return castingAbility.abilityName;
        }
    }
    public AttackSegment segment
    {
        get
        {
            return currentSegment;
        }
    }

    bool init = false;
    public override void enter()
    {
        segments = castingAbility.cast(mover);
        attackMachine = new StateMachine<AttackStageState>(getNextState);
        init = true;
    }

    public override void exit(bool expired)
    {
        castingAbility.startCooldown();
        attackMachine.exit();
        currentSegment.exitSegment();
    }
    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        if (inp.jump && mover.grounded)
        {
            mover.jump();
        }

        attackMachine.tick();



    }
    public override StateTransition transition()
    {
        if (mover.posture.isStunned)
        {
            float remaining = currentSegment.remainingWindDown();
            if (remaining > 0)
            {
                StunnedState stun = new StunnedState(mover, remaining);
                mover.GetComponent<Cast>().setTarget(stun);

                return new StateTransition(stun, true);
            }
            else
            {
                return new StateTransition(new StunnedState(mover), true);
            }

        }
        if (mover.input.cancel && currentSegment.inWindup())
        {
            return new StateTransition(new FreeState(mover), true);
        }
        attackMachine.transition();
        if (ended)
        {
            return new StateTransition(new FreeState(mover), true);
        }
        return base.transition();
    }

    AttackStageState getNextState()
    {
        Cast c = mover.GetComponent<Cast>();
        AttackStageState s = null;
        System.Action nextSegment = () =>
        {
            mover.maybeSnapRotation(mover.input);
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


