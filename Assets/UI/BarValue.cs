using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface BarValue
{
    public struct BarData
    {
        public Color color;
        public float fillPercent;
    }
    public BarData getBarFill();
}
