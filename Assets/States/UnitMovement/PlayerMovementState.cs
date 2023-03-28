public abstract class PlayerMovementState : State
{
    protected UnitMovement mover;
    public PlayerMovementState(UnitMovement m) : base()
    {
        mover = m;
    }
    public PlayerMovementState(UnitMovement m, float t) : base(t)
    {
        mover = m;
    }

}
