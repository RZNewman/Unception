using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitRingInd : NetworkBehaviour
{
    public GameObject indicator;


    List<Color> colorList = new List<Color>();

    public void addColor(Color c)
    {
        colorList.Add(c);
        RpcSpawnColors(colorList);
    }
    public void removeColor(Color c)
    {
        colorList.Remove(c);
        RpcSpawnColors(colorList);
    }


    List<GameObject> instances = new List<GameObject> ();
    [ClientRpc]
    void RpcSpawnColors(List<Color> colors)
    {
        foreach(GameObject instance in instances)
        {
            Destroy(instance);
        }
        float offset = 0;
        foreach (Color color in colors)
        {
            GameObject o = Instantiate(indicator, GetComponentInChildren<Size>().transform);
            instances.Add(o);
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
