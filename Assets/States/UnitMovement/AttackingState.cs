using System.Collections.Generic;

public class AttackingState : PlayerMovementState
{
    Ability castingAbility;

    StateMachine<PlayerMovementState> attackMachine;
    bool ended = false;

    List<PlayerMovementState> currentStates;


    public AttackingState(UnitMovement m, Ability atk) : base(m)
    {
        castingAbility = atk;
    }

    public override void enter()
    {
        currentStates = castingAbility.cast(mover);
        attackMachine = new StateMachine<PlayerMovementState>(getNextState);

    }

    public override void exit(bool expired)
    {
        castingAbility.startCooldown();
        attackMachine.exit();
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

    PlayerMovementState getNextState()
    {
        if (currentStates.Count > 0)
        {
            PlayerMovementState s = currentStates[0];
            currentStates.RemoveAt(0);
            return s;
        }
        ended = true;
        return new WindState(mover, 1f);
    }



}
