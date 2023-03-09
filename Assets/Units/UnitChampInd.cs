using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitChampInd : NetworkBehaviour
{
    public GameObject indicator;

    [SyncVar]
    public bool hasIndicator;
    [SyncVar]
    public Color color;
    void Start()
    {
        if (hasIndicator)
        {
            GameObject o = Instantiate(indicator, GetComponentInChildren<Size>().transform);
            foreach (ColorIndividual c in o.GetComponentsInChildren<ColorIndividual>())
            {
                c.setColor(color);
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
