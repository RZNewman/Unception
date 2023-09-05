using UnityEngine;
using static UiBarBasic;

public interface BarValue
{
    public struct BarData
    {
        public Color color;
        public float fillPercent;
        public bool active;
        public string text;
        public BarSegment[] segments;
    }
    public BarData getBarFill();
}
