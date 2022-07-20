using static UnitControl;

public abstract class AttackState : PlayerMovementState
{

    public AttackState(UnitMovement m) : base(m)
    {
        moveMultiplier = 0.5f;
        lookMultiplier = 0.5f;
    }
    public AttackState(UnitMovement m, float t) : base(m, t)
    {
        moveMultiplier = 0.5f;
        lookMultiplier = 0.5f;
    }

    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;


        mover.rotate(inp, lookMultiplier);
        mover.move(inp, moveMultiplier, moveMultiplier);


    }
}
