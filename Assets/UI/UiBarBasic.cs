using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class UiBarBasic : MonoBehaviour
{
    public RectTransform foreground;
    public GameObject barItem;

    public struct BarSegment
    {
        public Color color;
        public float percent;
    }

    List<GameObject> instances = new List<GameObject>();
    public void set(params BarSegment[] segments)
    {
        for (int i = 0; i < segments.Length; i++)
        {

            BarSegment seg = segments[i];
            Vector2 size;
            if (i >= instances.Count)
            {
                instances.Add(Instantiate(barItem, foreground));
            }
            Image img = instances[i].GetComponent<Image>();
            img.color = seg.color;
            size = img.rectTransform.sizeDelta;
            size.x = seg.percent * 100;
            img.rectTransform.sizeDelta = size;
        }
    }
}
