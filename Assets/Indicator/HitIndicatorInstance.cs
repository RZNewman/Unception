using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;

public abstract class HitIndicatorInstance : IndicatorInstance
{
    protected HitInstanceData data;
    public void setSource(HitInstanceData d)
    {
        data = d;
        setSize();
    }
}
