using System.Collections.Generic;
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
        AttackStageState s;
        bool enteredSegment = false;
        if (!init)
        {
            currentSegment = segments[0];
            mover.GetComponent<Cast>().buildIndicator(currentSegment.states);
            s = currentSegment.nextState();
            enteredSegment = true;
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
                    return new WindState(mover, 1f);
                }
                currentSegment = segments[0];
                mover.GetComponent<Cast>().buildIndicator(currentSegment.states);
                s = currentSegment.nextState();
                enteredSegment = true;
            }
        }
        if (!enteredSegment)
        {
            mover.GetComponent<Cast>().nextStage();
        }
        return s;
    }


}


