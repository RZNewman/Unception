using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IndicatorHolder
{
    public static readonly float IndicatorHeightOffset = 0.05f;
    public struct IndicatorLocalLook
    {
        public bool shouldOverride;
        public Vector3 newForward;
    }

    public abstract IndicatorLocalLook pointOverride(Vector3 fowardPlanar, Vector3 groundNormal);

    public abstract Vector3 indicatorPosition(Vector3 forward);

    public abstract float offsetMultiplier();
}
