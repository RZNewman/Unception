public class JumpsquatState : PlayerMovementState
{
    public JumpsquatState(UnitMovement m, float t) : base(m, t)
    {

    }
    public override void exit(bool expired)
    {
        if (expired)
        {
            mover.jump();
        }

    }
    public override StateTransition transition()
    {
        if (mover.posture.isStunned)
        {
            return new StateTransition(new StunnedState(mover), true);
        }
        if (!mover.grounded)
        {
            return new StateTransition(new FreeState(mover), true);
        }
        return base.transition();
    }


}
