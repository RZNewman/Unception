using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateHit;

public abstract class HitIndicatorInstance : IndicatorInstance
{
    protected HitInstanceData data;
    protected ShapeData shapeData;
    public void setSource(HitInstanceData d, ShapeData sData)
    {
        data = d;
        shapeData = sData;
        setSize();
    }
    protected override float getThreat()
    {
        return data.powerByStrength / GlobalPlayer.gPlay.localPowerThreat;
    }

    protected override bool willStagger()
    {
        return data.stagger >= GlobalPlayer.gPlay.localStunThreat;
    }
}
