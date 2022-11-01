using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;
using static RewardManager;

public class UiAbilityDetails : MonoBehaviour
{
    public Text title;
    public Text powerTotal;
    public Text power;
    public Text quality;
    public Text castTime;
    public Text cooldown;
    public Text charges;
    public UiSegmentPanel segmentPanel;

    PlayerGhost player;

    public void setDetails(AttackBlockFilled filled)
    {
        if (!player)
        {
            player = FindObjectOfType<PlayerGhost>();
        }
        title.text = filled.flair.name;
        power.text = Power.displayPower(filled.instance.power);
        quality.text = qualitySymbol(filled.instance.quality);
        quality.color = colorQuality(filled.instance.quality);
        powerTotal.text = Power.displayPower(filled.instance.power * qualityPercent(filled.instance.quality));
        castTime.text = filled.instance.castTime.ToString();
        cooldown.text = filled.instance.cooldown.ToString();
        charges.text = filled.instance.charges.ToString();


        segmentPanel.clearLabels();
        SegmentInstanceData prime = filled.instance.segments[0];
        segmentPanel.hitType.text = prime.hit.type.ToString();
        float damage = prime.hit.damageMult * Power.damageFalloff(filled.instance.power, player.power);
        segmentPanel.addLabel("Damage", damage);
        segmentPanel.addLabel("DPS", damage / prime.castTime);
        segmentPanel.addLabel("Windup", prime.windup.duration);
        segmentPanel.addLabel("Winddown", prime.winddown.duration);
        //Turnspeeds?
        segmentPanel.addLabel("Length", prime.hit.length);
        segmentPanel.addLabel("Width", prime.hit.width);
        segmentPanel.addLabel("Knockback", prime.hit.knockback);
        segmentPanel.addLabel("Stagger", prime.hit.stagger);
    }



}
