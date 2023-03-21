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

public class WindState : AttackStageState, BarValue
{
    bool isWinddown;
    GameObject groundTarget;
    GroundSearchParams groundSearch;

    public WindState(UnitMovement m) : base(m)
    {
        //Only for defaults
        isWinddown = false;
    }

    public WindState(UnitMovement m, WindInstanceData d, bool winddown = false) : base(m, d.duration)
    {
        isWinddown = winddown;
        moveMultiplier = d.moveMult;
        lookMultiplier = d.turnMult;
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
            groundTarget.GetComponent<FloorNormal>().setGround(groundSearch);
            groundTarget.GetComponent<GroundTarget>().setTarget(mover.lookWorldPos, 4.0f * mover.GetComponent<Power>().scaleSpeed() * lookMultiplier);
        }
        mover.rotate(inp, false, lookMultiplier);
        mover.move(inp, moveMultiplier);


    }


    public override void exit(bool expired)
    {
        mover.GetComponent<Cast>().removeTarget(this);
    }

    public override Cast.IndicatorOffsets GetIndicatorOffsets()
    {
        return new Cast.IndicatorOffsets
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
        return new BarValue.BarData
        {
            color = !isWinddown ? Color.cyan : new Color(0, 0.6f, 1),
            fillPercent = Mathf.Clamp01(!isWinddown ? 1 - (currentDurration / maxDuration) : currentDurration / maxDuration),
            active = true,
        };
    }
    public void setGroundTarget(GameObject t, GroundSearchParams s)
    {
        groundTarget = t;
        groundSearch = s;
    }
}
