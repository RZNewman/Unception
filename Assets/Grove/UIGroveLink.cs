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

    Color color;

    public void setVisuals(AttackFlair f, bool isNest)
    {
        color = f.color;
        Symbol symbolSource = FindObjectOfType<Symbol>();

        Image i = mainTarget.GetComponent<Image>();
        if (i)
        {
            i.color = color;
            i.gameObject.SetActive(isNest);
            return;
        }

        

        foreach (Image img in GetComponentsInChildren<Image>())
        {
            img.color = color;
            img.gameObject.SetActive(isNest);
            img.sprite = symbolSource.symbols[f.symbol];
        }
        Color c = color;
        c.a = 0.3f;
        foreach (ColorIndividual ind in GetComponentsInChildren<ColorIndividual>())
        {
            ind.setColor(c);
        }
        mainTarget.SetActive(isNest);
        
    }

    public void highlight(bool isHighlighted)
    {
        Color c = color;
        c.a = 0.3f;
        Color high = Color.white;
        high.a = 0.3f;
        foreach (ColorIndividual ind in GetComponentsInChildren<ColorIndividual>())
        {
            ind.setColor(isHighlighted ? high : c);
        }
    }
}
