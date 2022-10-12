using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IndicatorHolder
{
    public struct IndicatorLocalPoint
    {
        public bool shouldOverride;
        public Vector3 localPoint;
    }

    public abstract IndicatorLocalPoint pointOverride(Vector3 fowardPlanar, Vector3 groundNormal);

    public abstract Vector3 indicatorPosition(Vector3 forward);

    public abstract float offsetMultiplier();
}
