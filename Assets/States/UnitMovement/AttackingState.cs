using System.Collections.Generic;

public class AttackingState : PlayerMovementState
{
    Ability castingAbility;

    StateMachine<AttackStageState> attackMachine;
    bool ended = false;

    List<AttackStageState> currentStates;


    public AttackingState(UnitMovement m, Ability atk) : base(m)
    {
        castingAbility = atk;
    }

    bool init = false;
    public override void enter()
    {
        currentStates = castingAbility.cast(mover);
        mover.GetComponent<Cast>().buildIndicator(currentStates);
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
        if (currentStates.Count > 0)
        {
            AttackStageState s = currentStates[0];
            currentStates.RemoveAt(0);
            if (init)
            {
                mover.GetComponent<Cast>().nextStage();
            }

            return s;
        }
        ended = true;
        return new WindState(mover, 1f);
    }



}
