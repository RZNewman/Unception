using static UnitControl;

public class StunnedState : PlayerMovementState
{
    public StunnedState(UnitMovement m) : base(m)
    {

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
        if (!mover.posture.isStunned)
        {
            return new StateTransition(new FreeState(mover), true);
        }
        return base.transition();
    }
}
