using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitChampInd : NetworkBehaviour
{
    public GameObject indicator;

    public readonly SyncList<Color> colors = new SyncList<Color>();
    void Start()
    {
        float offset = 0;
        foreach (Color color in colors)
        {
            GameObject o = Instantiate(indicator, GetComponentInChildren<Size>().transform);
            o.transform.localPosition += offset * Vector3.up;
            Color colorAlpha = color;
            colorAlpha.a = 0.7f;
            foreach (ColorIndividual c in o.GetComponentsInChildren<ColorIndividual>())
            {
                c.setColor(colorAlpha);
            }

            offset += 0.15f;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}