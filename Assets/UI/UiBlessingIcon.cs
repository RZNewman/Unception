using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;

public class UiBlessingIcon : MonoBehaviour
{
    public Text identifier;
    public Image symbol;

    TriggerDataInstance triggerDataInstance;
    // Start is called before the first frame update
    public void setFill(TriggerData t, Inventory inv)
    {
        Symbol symbolSource = FindObjectOfType<Symbol>();
        Color partialColor = t.flair.color;
        partialColor.a = 0.4f;

        symbol.sprite = symbolSource.symbols[t.flair.symbol];
        symbol.color = t.flair.color;
        identifier.color = partialColor;
        identifier.text = t.flair.identifier;

        triggerDataInstance = (TriggerDataInstance)inv.fillBlock(t, t.conditions.triggerStrength);
    }
    public TriggerDataInstance triggerInstance
    {
        get
        {
            return triggerDataInstance;
        }
    }
}
