using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartAssignment : MonoBehaviour
{
    public List<PartPicker> pickers;
    public List<ScaleGroup> scalerGroups;

    [System.Serializable]
    public struct ScaleGroup
    {
        public List<PartScaler> scalers;
        public bool shared;

        public IEnumerable<Vector3> random()
        {
            if(shared || Random.value < 0.8f)
            {
                //symmetrical
                return Enumerable.Repeat(scalers[0].random(),scalers.Count);
                
            }
            else
            {
                return scalers.Select(scale => scale.random());
            }
            
        }
    }

    public UnitVisuals getVisuals()
    {
        return new UnitVisuals
        {
            parts = pickers.Select(pick => Random.Range(0, pick.partCount())).ToArray(),
            colors = Enumerable.Repeat(0, 16).Select(i => randomColor()).ToArray(),
            scales = scalerGroups.Select(group => group.scalers[0].random()).ToArray(),
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
        if (vis.scales.Length > 0)
        {
            int i = 0;
            for (int j = 0; j < scalerGroups.Count; j++)
            {
                for (int k = 0; k < scalerGroups[j].scalers.Count; k++)
                {
                    scalerGroups[j].scalers[k].scale(vis.scales[i]);
                    i++;
                }
            }
        }

    }

    Color randomColor()
    {
        return Random.ColorHSV();
    }
}
