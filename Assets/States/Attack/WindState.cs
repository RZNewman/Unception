using Mirror;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;
using System.Collections.Generic;
using static UnitControl;
using static AttackUtils;
using static FloorNormal;
using static Size;
using static SpellSource;

public class WindState : AttackStageState, BarValue
{
    bool isWinddown;
    WindInstanceData windData;
    SpellSource groundTarget;


    public WindState(UnitMovement m) : base(m)
    {
        //Only for defaults
        isWinddown = false;
    }

    public WindState(UnitMovement m, WindInstanceData d, bool winddown = false) : base(m, d.duration)
    {
        isWinddown = winddown;
        windData = d;
    }

    public override void enter()
    {
        mover.GetComponent<Cast>().setTarget(this);


    }
    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        if (groundTarget)
        {
            groundTarget.setTarget(mover.lookWorldPos, 4.0f * mover.GetComponent<Power>().scaleSpeed() * windData.turnMult);
        }
        mover.rotate(inp, false, windData.turnMult, windData.turnspeedCast);
        mover.move(inp, windData.moveMult, windData.movespeedCast);


    }


    public override void exit(bool expired)
    {
        mover.GetComponent<Cast>().removeTarget(this);
    }

    public override IndicatorOffsets GetIndicatorOffsets()
    {
        return new IndicatorOffsets
        {
            distance = Vector3.zero,
            time = currentDurration,
        };
    }
    public float remainingDuration
    {
        get
        {
            return currentDurration;
        }
    }

    public BarValue.BarData getBarFill()
    {
        Color c = !isWinddown ? Color.cyan : new Color(0, 0.6f, 1);
        c.a = 0.6f;
        return new BarValue.BarData
        {
            color = c,
            fillPercent = Mathf.Clamp01(!isWinddown ? 1 - (currentDurration / maxDuration) : currentDurration / maxDuration),
            active = true,
            text = mover.currentAbilityName(),
        };
    }
    public void setGroundTarget(SpellSource t)
    {
        groundTarget = t;
    }

    protected override float tickSpeedMult()
    {
        return windData.castSpeedMultiplier;
    }
}
