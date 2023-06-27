using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateAttack;
using static GenerateDash;
using static GenerateHit;

public class Cast : MonoBehaviour, BarValue
{
    BarValue target;

    public BarValue.BarData getBarFill()
    {
        if (target == null)
        {
            return new BarValue.BarData
            {
                color = Color.cyan,
                fillPercent = 0,
                active = false,
            };
        }
        return target.getBarFill();
    }

    public void setTarget(BarValue s)
    {
        target = s;
    }
    public void removeTarget(BarValue s)
    {
        if (target == s)
        {
            target = null;
        }

    }






}
