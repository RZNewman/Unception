using static UnitControl;
using static Cast;
using static SpellSource;

public abstract class AttackStageState : PlayerMovementState
{
    public int[] indicatorIds = new int[0];

    public virtual float selfPercent
    {
        get
        {
            return 0;
        }
    }
    public AttackStageState(UnitMovement m) : base(m)
    {

    }
    public AttackStageState(UnitMovement m, float t) : base(m, t)
    {
    }

    public abstract IndicatorOffsets GetIndicatorOffsets();


}
