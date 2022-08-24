using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IndicatorHolder
{
    public abstract Vector3 indicatorPosition(Vector3 forward);

    public abstract float offsetMultiplier();
}
