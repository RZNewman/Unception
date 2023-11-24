using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIGroveLink : MonoBehaviour
{
    public GameObject mainTarget;
    //public GameObject auraTarget;

    public void setVisuals(Color c, bool isNest)
    {
        Image i = mainTarget.GetComponent<Image>();
        if (i)
        {
            i.color = c;
            i.gameObject.SetActive(isNest);
            return;
        }

        foreach(ColorIndividual ind in GetComponentsInChildren<ColorIndividual>())
        {
            c.a = 0.3f;
            ind.setColor(c);
        }
        mainTarget.SetActive(isNest);
        
    }
}
