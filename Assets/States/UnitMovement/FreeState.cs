using UnityEngine;
using static DashState;
using static GenerateDash;
using static UnitControl;

public class FreeState : PlayerMovementState
{
    public FreeState(UnitMovement m) : base(m)
    {

    }

    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        mover.rotate(inp);
        mover.move(inp);

    }
    public override StateTransition transition()
    {
        //Only hit on server, bc damage is dealt there
        if (mover.posture.isStunned)
        {
            return new StateTransition(new StunnedState(mover), true);
        }
        UnitInput inp = mover.input;

        AttackKey key = inp.popKey();


        while (key != AttackKey.None)
        {
            Ability a = mover.GetComponent<AbiltyList>().getAbility(key);
            //Debug.Log(key);
            //Debug.Log(a.ready);
            if (a.ready)
            {
                return new StateTransition(new AttackingState(mover, a), true);
            }
            key = inp.popKey();
        }


        Stamina s = mover.GetComponent<Stamina>();
        if (inp.dash && s.stamina > Stamina.dashCost)
        {
            s.spendStamina(Stamina.dashCost);
            DashInstanceData o = mover.baseDash();
            return new StateTransition(new DashState(mover, o), true);
        }
        if (inp.jump && mover.grounded)
        {
            return new StateTransition(new JumpsquatState(mover, mover.props.jumpsquatTime), true);
        }
        return base.transition();
    }


}
