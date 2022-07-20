using UnityEngine;

public interface BarValue
{
    public struct BarData
    {
        public Color color;
        public float fillPercent;
        public bool active;
    }
    public BarData getBarFill();
}
