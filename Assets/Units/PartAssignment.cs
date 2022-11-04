using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartAssignment : MonoBehaviour
{
    public List<PartPicker> pickers;
    

    public UnitVisuals getVisuals()
    {
        return new UnitVisuals
        {
            parts = pickers.Select(pick => Random.Range(0, pick.partCount())).ToArray(),
            colors = Enumerable.Repeat(0, 16).Select(i => randomColor()).ToArray(),
        };
    }

    private void Start()
    {
        setVisuals(GetComponentInParent<UnitPropsHolder>().props.visuals);
    }

    void setVisuals(UnitVisuals vis)
    {
        if (vis.parts.Length > 0)
        {
            for (int i = 0; i < vis.parts.Length; i++)
            {
                pickers[i].pickPart(vis.parts[i], vis.colors);
            }
        }
        
    }

    Color randomColor()
    {
        return Random.ColorHSV();
    }
}
