using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTarget : MonoBehaviour, IndicatorHolder
{
    public float height;
    public Vector3 indicatorPosition(Vector3 forward)
    {
        return Vector3.down * height;
    }
    public float offsetMultiplier()
    {
        return 0.0f;
    }


}
