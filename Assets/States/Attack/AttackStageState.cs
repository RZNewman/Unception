using static UnitControl;
using static Cast;

public abstract class AttackStageState : PlayerMovementState
{

    public AttackStageState(UnitMovement m) : base(m)
    {

    }
    public AttackStageState(UnitMovement m, float t) : base(m, t)
    {
    }

    public abstract IndicatorOffsets GetIndicatorOffsets();


}
