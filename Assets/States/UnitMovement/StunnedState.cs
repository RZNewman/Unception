using UnityEngine;
using static UnitControl;

public class StunnedState : PlayerMovementState, BarValue
{
    public StunnedState(UnitMovement m) : base(m)
    {

    }

    public StunnedState(UnitMovement m, float t) : base(m, t)
    {
        //used for sturn duration overrides
    }

    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        //no look change
        //defaultLook(inp);
        mover.move(UnitInput.zero(), 0.2f);

    }

    public override StateTransition transition()
    {
        if (currentDurration <= 0 && !mover.posture.isStunned)
        {
            return new StateTransition(new FreeState(mover), true);
        }
        return base.transition();
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = GameColors.Stunned,
            fillPercent = Mathf.Clamp01(1 - (currentDurration / maxDuration)),
            active = true,
        };
    }
}
