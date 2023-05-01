using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;

public class StatModPanel : MonoBehaviour
{
    public GameObject StatModLabelPre;

    public void fill(AttackBlockFilled block,float playerPower)
    {
        clearLabels();

        create().populate("DPS", 
            Power.displayPower(block.instance.dps(playerPower)),
            block.generationData.getInfo(Stat.DamageMult),
            "Damage "+Power.displayPower(block.instance.damage(playerPower)));

        create().populate("CD", Power.displayPower(block.getCooldownDisplay(playerPower)), block.generationData.getInfo(Stat.Cooldown));
        create().populate("Charges", Power.displayPower(block.getCharges()), block.generationData.getInfo(Stat.Charges));
        create().populate("Cast", Power.displayPower(block.instance.castTimeDisplay(playerPower)), block.generationData.getInfo(Stat.Haste));
        create().populate("Turn", Power.displayPower(block.instance.avgTurn()), block.generationData.getInfo(Stat.TurnspeedCast));
        create().populate("Move", Power.displayPower(block.instance.avgMove()), block.generationData.getInfo(Stat.MovespeedCast));

        create().populate("Length", Power.displayPower(block.instance.avgLength()), block.generationData.getInfo(Stat.Length));
        create().populate("Width", Power.displayPower(block.instance.avgWidth()), block.generationData.getInfo(Stat.Width));
        create().populate("Range", Power.displayPower(block.instance.avgRange()), block.generationData.getInfo(Stat.Range));
        create().populate("KB", Power.displayPower(block.instance.avgKback()), block.generationData.getInfo(Stat.Knockback));
        create().populate("KUp", Power.displayPower(block.instance.avgKup()), block.generationData.getInfo(Stat.Knockup));
        create().populate("Stun", Power.displayPower(block.instance.avgStagger()), block.generationData.getInfo(Stat.Stagger));

        

    }

    StatModLabel create()
    {
        return Instantiate(StatModLabelPre, transform).GetComponent<StatModLabel>();
    }
    public void clearLabels()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
