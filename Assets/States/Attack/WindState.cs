using Mirror;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;
using System.Collections.Generic;
using static UnitControl;

public class WindState : AttackStageState, BarValue
{
    bool ending;


    public WindState(UnitMovement m, float d) : base(m, d)
    {
        //Only for defaults
        ending = false;
    }

    public WindState(UnitMovement m, WindInstanceData d, bool end = false) : base(m, d.duration)
    {
        ending = end;
        moveMultiplier = d.moveMult;
        lookMultiplier = d.turnMult;
    }

    public override void enter()
    {
        GameObject target = mover.getSpawnBody();
        target.GetComponentInParent<Cast>().setTarget(this);


    }
    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;


        mover.rotate(inp, lookMultiplier);
        mover.move(inp, moveMultiplier, moveMultiplier);


    }


    public override void exit(bool expired)
    {
        GameObject target = mover.getSpawnBody();
        target.GetComponentInParent<Cast>().removeTarget();
    }

    public override Cast.IndicatorOffsets GetIndicatorOffsets()
    {
        return new Cast.IndicatorOffsets
        {
            distance = Vector3.zero,
            time = currentDurration,
        };
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = !ending ? Color.cyan : new Color(0, 0.6f, 1),
            fillPercent = Mathf.Clamp01(!ending ? 1 - (currentDurration / maxDuration) : currentDurration / maxDuration),
            active = true,
        };
    }
}
