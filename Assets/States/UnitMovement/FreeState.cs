using UnityEngine;
using static DashState;
using static GenerateAttack;
using static GenerateDash;
using static UnitControl;
using static Utils;
using static AttackUtils;

public class FreeState : PlayerMovementState
{
    public FreeState(UnitMovement m) : base(m)
    {

    }

    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        if (inp.jump)
        {
            mover.tryJump();
        }

        mover.rotate(inp);
        mover.move(inp);

    }
    public override StateTransition transition()
    {
        if (mover.isIncapacitated)
        {
            return new StateTransition(new StunnedState(mover), true);
        }
        UnitInput inp = mover.input;

        Optional<ItemSlot> key = inp.popKey();


        while (key.HasValue)
        {
            Ability a = mover.GetComponent<AbilityManager>().getAbility(key.Value);
            //Debug.Log(key);
            //Debug.Log(a.ready);
            if (a != null && a.ready)
            {
                return new StateTransition(new AttackingState(mover, a), true);
            }
            key = inp.popKey();
        }

        if(inp.consumeRecall() && mover.grounded)
        {
            return new StateTransition(new ChannelState(mover, 3f, "Returning...", () =>
            {
                mover.GetComponent<UnitPropsHolder>().owningPlayer.GetComponent<PlayerGhost>().transitionShip(true);
            } ), true);
        }


        Stamina s = mover.GetComponent<Stamina>();
        if (inp.dash && s.stamina > Stamina.dashCost)
        {
            s.spendStamina(Stamina.dashCost);
            DashInstanceData o = mover.baseDash();
            //if (!mover.grounded)
            //{
            //    float power = mover.GetComponent<Power>().power;
            //    SpawnBuff(mover.transform, GenerateBuff.BuffMode.Shield, power, 0.3f, power * 2.0f);
            //}
            return new StateTransition(new DashState(mover, o, false), true);
        }

        return base.transition();
    }


}
