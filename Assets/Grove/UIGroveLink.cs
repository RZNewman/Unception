using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static GenerateAttack;

public class UIGroveLink : MonoBehaviour
{
    public GameObject mainTarget;
    //public GameObject auraTarget;

    public void setVisuals(AttackFlair f, bool isNest)
    {
        Color c = f.color;
        Symbol symbolSource = FindObjectOfType<Symbol>();

        Image i = mainTarget.GetComponent<Image>();
        if (i)
        {
            i.color = c;
            i.gameObject.SetActive(isNest);
            return;
        }

        

        foreach (Image img in GetComponentsInChildren<Image>())
        {
            img.color = c;
            img.gameObject.SetActive(isNest);
            img.sprite = symbolSource.symbols[f.symbol];
        }
        c.a = 0.3f;
        foreach (ColorIndividual ind in GetComponentsInChildren<ColorIndividual>())
        {
            ind.setColor(c);
        }
        mainTarget.SetActive(isNest);
        
    }
}
