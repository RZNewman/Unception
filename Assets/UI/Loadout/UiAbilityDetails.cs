using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;
using static RewardManager;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UiAbilityDetails : MonoBehaviour
{
    public Text title;
    public Text powerTotal;
    public Text power;
    public Text quality;
    public Text castTime;
    public Text cooldown;
    public Text charges;
    public Text damage;
    public Text dps;
    public Text effect;
    public Text eps;
    public GameObject segPanelHolder;
    public GameObject segPanelPre;
    

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
        castTime.text = Power.displayPower(filled.instance.castTime);
        cooldown.text = Power.displayPower(filled.instance.cooldown);
        charges.text = Power.displayPower(filled.instance.charges);

        damage.text = Power.displayPower(filled.instance.damage(player.power));
        dps.text = Power.displayPower(filled.instance.dps(player.power));
        effect.text = Power.displayPower(filled.instance.effect);
        eps.text = Power.displayPower(filled.instance.eps);

        foreach (Transform child in segPanelHolder.transform)
        {
            Destroy(child.gameObject);
        }
        foreach(SegmentInstanceData seg in filled.instance.segments)
        {
            buildSegment(seg, player.power);
        }
    }

    void buildSegment(SegmentInstanceData instance, float power)
    {
        GameObject panel = Instantiate(segPanelPre, segPanelHolder.transform);
        UiSegmentPanel segmentPanel = panel.GetComponent<UiSegmentPanel>();

        segmentPanel.hitType.text = instance.hit.type.ToString();
        segmentPanel.addLabel("Effect", instance.effectPower);
        segmentPanel.addLabel("EPS", instance.eps);
        segmentPanel.addLabel("Damage", instance.damage(power));
        segmentPanel.addLabel("DPS", instance.dps(power));
        
        segmentPanel.addLabel("Windup", instance.windup.duration);
        segmentPanel.addLabel("Winddown", instance.winddown.duration);
        segmentPanel.addLabel("Move", instance.avgMove);
        segmentPanel.addLabel("Turn", instance.avgTurn);

        segmentPanel.addLabel("Length", instance.hit.length);
        segmentPanel.addLabel("Width", instance.hit.width);
        segmentPanel.addLabel("Knockback", instance.hit.knockback);
        segmentPanel.addLabel("KB Dir", instance.hit.knockBackDirection.ToString());
        segmentPanel.addLabel("Stagger", instance.hit.stagger);
        
        if(instance.dash!= null)
        {
            segmentPanel.addLabel("Dash Len", instance.dash.distance);
            segmentPanel.addLabel("Dash Speed", instance.dash.speed);
            segmentPanel.addLabel("Dash Dir", instance.dash.control.ToString());
        }
        if (instance.repeat != null)
        {
            segmentPanel.addLabel("Repeats", instance.repeat.repeatCount);
            segmentPanel.addLabel("Repeat delay", instance.windRepeat.duration);
        }
    }



}
