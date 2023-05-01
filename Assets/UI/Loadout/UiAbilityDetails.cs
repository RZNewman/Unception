using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;
using static RewardManager;
using static Naming;

public class UiAbilityDetails : MonoBehaviour
{
    public Text title;
    public Text titleSlot;
    public Text powerTotal;
    public Text power;
    public Image slotIcon;
    public Image qualityBG;

    public StatModPanel statPanel;


    PlayerGhost player;

    public void setDetails(AttackBlockFilled filled)
    {
        if (!player)
        {
            player = FindObjectOfType<PlayerGhost>();
        }

        title.text = filled.flair.name;
        titleSlot.text = slotPhysical(filled.slot)+" of";
        power.text = Power.displayExaggertatedPower(filled.instance.power);
        powerTotal.text = Power.displayExaggertatedPower(filled.instance.actingPower);
        slotIcon.sprite = FindObjectOfType<Symbol>().fromSlot(filled.slot ?? ItemSlot.Main);
        qualityBG.color = colorQuality(filled.instance.quality);
        statPanel.fill(filled, player.power);

    }

    void buildSegment(SegmentInstanceData instance, float power)
    {


        //if (instance.dash != null)
        //{
        //    segmentPanel.addLabel("Dash Len", instance.dash.distance);
        //    segmentPanel.addLabel("Dash Speed", instance.dash.speed);
        //    segmentPanel.addLabel("Dash Dir", instance.dash.control.ToString());
        //}
        //if (instance.repeat != null)
        //{
        //    segmentPanel.addLabel("Repeats", instance.repeat.repeatCount);
        //    segmentPanel.addLabel("Repeat delay", instance.windRepeat.durationDisplay(power));
        //}
        //if (instance.buff != null)
        //{
        //    segmentPanel.addLabel("Buff Dur.", instance.buff.durationDisplay(power));
        //    string label = instance.buff.type == GenerateBuff.BuffType.Buff ? "Buff " : "Debuff ";
        //    segmentPanel.addLabel(label, instance.buff.stats.First().Key.ToString());
        //    segmentPanel.addLabel("Buff Value", instance.buff.stats.First().Value);
        //}

    }



}
