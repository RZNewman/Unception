using static UnitControl;
using static Cast;

public abstract class AttackStageState : PlayerMovementState
{

    public AttackStageState(UnitMovement m) : base(m)
    {
        moveMultiplier = 0.5f;
        lookMultiplier = 0.5f;
    }
    public AttackStageState(UnitMovement m, float t) : base(m, t)
    {
        moveMultiplier = 0.5f;
        lookMultiplier = 0.5f;
    }

    public abstract IndicatorOffsets GetIndicatorOffsets();


}
