using UnityEngine;
using static DashState;
using static GenerateAttack;
using static GenerateDash;
using static UnitControl;
using static Utils;

public class FreeState : PlayerMovementState
{
    public FreeState(UnitMovement m) : base(m)
    {

    }

    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        if (inp.consumeJump())
        {
            mover.tryJump();
        }

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

        Optional<ItemSlot> key = inp.popKey();


        while (key.HasValue)
        {
            Ability a = mover.GetComponent<AbiltyManager>().getAbility(key.Value);
            //Debug.Log(key);
            //Debug.Log(a.ready);
            if (a != null && a.ready)
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
            return new StateTransition(new DashState(mover, o, false), true);
        }

        return base.transition();
    }


}
