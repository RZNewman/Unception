public abstract class PlayerMovementState : State
{
    protected UnitMovement mover;
    public float moveMultiplier = 1f;
    public float lookMultiplier = 1f;
    public PlayerMovementState(UnitMovement m) : base()
    {
        mover = m;
    }
    public PlayerMovementState(UnitMovement m, float t) : base(t)
    {
        mover = m;
    }

}
