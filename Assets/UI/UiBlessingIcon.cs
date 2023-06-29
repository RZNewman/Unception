using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBlessingIcon : MonoBehaviour
{
    public Text identifier;
    public Image symbol;
    // Start is called before the first frame update
    public void setFill(AttackTrigger t)
    {
        Symbol symbolSource = FindObjectOfType<Symbol>();
        Color partialColor = t.flair.color;
        partialColor.a = 0.4f;

        symbol.sprite = symbolSource.symbols[t.flair.symbol];
        symbol.color = t.flair.color;
        identifier.color = partialColor;
        identifier.text = t.flair.identifier;
    }
}
