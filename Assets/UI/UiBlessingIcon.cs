using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBlessingIcon : MonoBehaviour
{
    public Text identifier;
    public Image symbol;

    AttackBlockInstance attackInst;
    AttackTrigger trigger;
    // Start is called before the first frame update
    public void setFill(AttackTrigger t, Inventory inv)
    {
        Symbol symbolSource = FindObjectOfType<Symbol>();
        Color partialColor = t.flair.color;
        partialColor.a = 0.4f;

        symbol.sprite = symbolSource.symbols[t.flair.symbol];
        symbol.color = t.flair.color;
        identifier.color = partialColor;
        identifier.text = t.flair.identifier;

        attackInst = inv.fillBlock(t.block, t.conditions.triggerStrength);
        trigger = t;
    }

    public AttackTrigger attackTrigger
    {
        get
        {
            return trigger;
        }
    }
    public AttackBlockInstance blockInstance
    {
        get
        {
            return attackInst;
        }
    }
}
