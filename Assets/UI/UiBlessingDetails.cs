using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AttackSegment;
using static GenerateAttack;
using static RewardManager;

public class UiBlessingDetails : MonoBehaviour
{
    public TMP_Text description;
    public Text powerTotal;
    public Text power;

    public TMP_Text shape;
    public StatModPanel statPanel;

    PlayerGhost player;

    public void setDetails(UiBlessingIcon icon)
    {
        setDetails(icon.blockInstance, icon.attackTrigger.conditions);
    }


    public void setDetails(AttackBlockInstance filled, TriggerConditions conditions)
    {
        if (!player)
        {
            player = FindObjectOfType<PlayerGhost>();
        }

        description.text = descriptionText(conditions, filled.flair);
        power.text = Power.displayExaggertatedPower(filled.instance.power);
        powerTotal.text = Power.displayExaggertatedPower(filled.instance.actingPower);
        shape.text = filled.instance.shapeDisplay();


        statPanel.fill(filled, player.power);

    }

    string descriptionText(TriggerConditions conditions, AttackFlair flair)
    {
        string prefix = conditions.trigger switch
        {
            Trigger.Always => "On Cooldown",
            Trigger.HitRecieved => "When hit",
            Trigger.HitGiven => "On hit",
            Trigger.Cast => "On cast",
            _ => "Whenever",
        };
        string target = conditions.location switch
        {
            SourceLocation.Body => "in front of you",
            SourceLocation.World => "at the target",
            SourceLocation.BodyFixed => "in the direction of the target",
            _ => "in some direction",
        };

        return string.Format("{0}, fire '{1}' {2}", prefix, flair.name, target);
    }

}
