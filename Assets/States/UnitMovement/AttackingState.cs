using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;

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
        mover.GetComponent<Cast>().killIndicators();
    }
    public override void tick()
    {
        base.tick();

        attackMachine.tick();



    }
    public override StateTransition transition()
    {
        if (mover.posture.isStunned)
        {
            return new StateTransition(new StunnedState(mover), true);
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
        AttackStageState s = null;
        bool enteredSegment = false;
        System.Action nextSegment = () =>
        {
            currentSegment = segments[0];
            currentSegment.enterSegment(mover);
            mover.GetComponent<Cast>().buildIndicator(currentSegment);

            s = currentSegment.nextState();
            enteredSegment = true;
        };
        if (!init)
        {
            nextSegment();
        }
        else
        {
            s = currentSegment.nextState();
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
        if (!enteredSegment)
        {
            mover.GetComponent<Cast>().nextStage();
        }
        return s;
    }


}


