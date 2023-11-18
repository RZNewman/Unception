using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIGroveLink : MonoBehaviour
{
    public GameObject colorTarget;

    public void setColor(Color c)
    {
        if (colorTarget)
        {
            colorTarget.GetComponent<Image>().color = c;
        }
        else
        {
            foreach(ColorIndividual ind in GetComponentsInChildren<ColorIndividual>())
            {
                c.a = 0.3f;
                ind.setColor(c);
            }
        }
    }
}
