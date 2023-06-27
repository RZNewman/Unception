using System.Collections.Generic;
using UnityEngine;
using static AttackSegment;
using static AttackUtils;
using static UnitControl;

public class AttackingState : PlayerMovementState
{
    Ability castingAbility;

    AttackMachine machine;

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
            return machine.segment;
        }
    }

    public override void enter()
    {
        machine = new AttackMachine(castingAbility, mover, new AttackMachine.CastingLocationData() { hardCast = true });
        mover.GetComponent<Cast>().addSource(machine, true);
    }

    public override void exit(bool expired)
    {
        machine.exit();
        mover.GetComponent<Cast>().removeSource(machine);
    }
    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        if (inp.jump && mover.grounded)
        {
            mover.jump();
        }

        machine.tick();



    }
    public override StateTransition transition()
    {
        if (mover.posture.isStunned)
        {
            float remaining = machine.remainingWindDown;
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
        if (mover.input.cancel && machine.inWindup)
        {
            return new StateTransition(new FreeState(mover), true);
        }
        machine.transition();
        if (machine.hasFinished)
        {
            return new StateTransition(new FreeState(mover), true);
        }
        return base.transition();
    }



}


