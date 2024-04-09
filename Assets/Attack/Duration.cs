using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Duration 
{
    public float remainingDuration
    {
        get;
    }
    public float maxDuration
    {
        get;
    }
    public float remainingPercent
    {
        get
        {
            return remainingDuration/maxDuration;
        }
    }
}
