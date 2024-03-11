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
    SpellSource[] groundTargets = new SpellSource[0];
    bool hardCast;

    public WindState(UnitMovement m) : base(m)
    {
        //Only for defaults
        isWinddown = false;
    }

    public WindState(UnitMovement m, WindInstanceData d, bool winddown, bool hardCasted) : base(m, d.duration)
    {
        isWinddown = winddown;
        windData = d;
        hardCast = hardCasted;
    }

    public override void enter()
    {
        if (hardCast)
        {
            mover.GetComponent<Cast>().setTarget(this);
        }
        


    }
    public override void tick()
    {
        base.tick();

        if (!hardCast)
        {
            return;
        }

        UnitInput inp = mover.input;

        if (groundTargets.Length >0 )
        {
            foreach(SpellSource source in groundTargets)
            {
                source.setTarget(mover.lookWorldPos, 4.0f * mover.GetComponent<Power>().scaleSpeed() * windData.turnMult);
            }
            
        }
        mover.rotate(inp, false, windData.turnMult, windData.turnspeedCast);
        mover.move(inp, windData.moveMult, windData.movespeedCast);


    }


    public override void exit(bool expired)
    {
        if (hardCast)
        {
            mover.GetComponent<Cast>().removeTarget(this);
        }
        
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
    public void setGroundTarget(SpellSource[] t)
    {
        groundTargets = t;
    }

    protected override float tickSpeedMult()
    {
        return windData.castSpeedMultiplier;
    }
}
