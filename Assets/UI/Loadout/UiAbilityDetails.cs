using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;
using static RewardManager;
using static Naming;
using TMPro;

public class UiAbilityDetails : MonoBehaviour
{
    public Text title;
    public Text titleSlot;
    public Text powerTotal;
    public Text power;

    public TMP_Text shape;
    public Image slotIcon;
    public Image qualityBG;

    public StatModPanel statPanel;


    PlayerGhost player;

    public void setDetails(AttackBlockInstance filled, AttackBlockInstance compare)
    {
        if (!player)
        {
            player = FindObjectOfType<PlayerGhost>();
        }

        title.text = filled.flair.name;
        titleSlot.text = slotPhysical(filled.slot) + " of";
        power.text = Power.displayExaggertatedPower(filled.instance.power);
        powerTotal.text = Power.displayExaggertatedPower(filled.instance.actingPower);
        shape.text = filled.instance.shapeDisplay();
        slotIcon.sprite = FindObjectOfType<Symbol>().fromSlot(filled.slot ?? ItemSlot.Main);
        qualityBG.color = colorQuality(filled.instance.quality);

        statPanel.fill(filled, player.power, compare);

    }






}
