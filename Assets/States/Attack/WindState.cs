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
using System;

public class WindState : AttackStageState, BarValue
{
    WindType type;
    WindInstanceData windData;
    SpellSource[] groundTargets = new SpellSource[0];
    bool hardCast;

    public enum WindType
    {
        Up,
        Down,
        Channel
    }

    public WindState(UnitMovement m) : base(m)
    {
        //Only for defaults
        type = WindType.Up;
    }

    public WindState(UnitMovement m, WindInstanceData d, WindType type, bool hardCasted) : base(m, d.duration)
    {
        this.type = type;
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
        Color c = type switch
        {
            WindType.Up => Color.cyan,
            WindType.Down => new Color(0, 0.6f, 1),
            WindType.Channel => new Color(0.4f, 1f, 1),
            _ => throw new NotImplementedException()
        };
        bool backwards = type switch
        {
            WindType.Down => true,
            _ => false
        };

        c.a = 0.6f;
        return new BarValue.BarData
        {
            color = c,
            fillPercent = Mathf.Clamp01(backwards ? currentDurration / maxDuration : 1 - (currentDurration / maxDuration)),
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
