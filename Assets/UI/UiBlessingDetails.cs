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
        TriggerDataInstance trigger = icon.triggerInstance;


        if (!player)
        {
            player = FindObjectOfType<PlayerGhost>();
        }

        description.text = descriptionText(trigger.conditions, trigger.flair);
        power.text = Power.displayExaggertatedPower(trigger.powerInstance);
        powerTotal.text = Power.displayExaggertatedPower(trigger.actingPower);
        shape.text = trigger.effect.shapeDisplay();


        statPanel.fill(trigger, player.power);

    }

    string descriptionText(TriggerConditions conditions, AttackFlair flair)
    {
        string prefix = conditions.trigger switch
        {
            Trigger.Always => "On cooldown",
            Trigger.HitRecieved => "When hit",
            Trigger.HitGiven => "On hit",
            Trigger.Cast => "On cast",
            _ => "Whenever",
        };
        if (conditions.triggerSlot.HasValue)
        {
            string slot = string.Format(" of your {0} skill", conditions.triggerSlot.Value);
            prefix += slot;
        }
        string target = conditions.location switch
        {
            SourceLocation.Body => "in front of you",
            SourceLocation.World => "at the target",
            SourceLocation.BodyFixed => "in the direction of the target",
            SourceLocation.WorldForward => "where you are aiming",
            _ => "in some direction",
        };

        return string.Format("{0}, fire '{1}' {2}", prefix, flair.name, target);
    }

}
