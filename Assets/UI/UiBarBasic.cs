using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBarBasic : MonoBehaviour
{
    public Image foreground;
    public Image foreground2;

    public struct BarSegment
    {
        public Color color;
        public float percent;
    }
    public void set(params BarSegment[] segments)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            BarSegment seg = segments[i];
            Vector2 size;
            switch (i)
            {
                case 0:
                    foreground.color = seg.color;
                    size = foreground.rectTransform.sizeDelta;
                    size.x = seg.percent * 100;
                    foreground.rectTransform.sizeDelta = size;
                    break;
                case 1:
                    foreground2.color = seg.color;
                    size = foreground2.rectTransform.sizeDelta;
                    size.x = seg.percent * 100;
                    foreground2.rectTransform.sizeDelta = size;
                    break;
            }
        }
    }
}
