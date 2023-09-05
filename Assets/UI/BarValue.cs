using UnityEngine;

public interface BarValue
{
    public struct BarData
    {
        public Color color;
        public float fillPercent;
        public Color color2;
        public float fillPercent2;
        public bool active;
        public string text;
    }
    public BarData getBarFill();
}
