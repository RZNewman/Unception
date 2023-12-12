using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;
using static AttackUtils;

public class GroundIndicatorVisuals : HitIndicatorInstance
{


    public GameObject edge;
    public GameObject progress;

    float diameter;
    // Start is called before the first frame update

    protected override void setSize()
    {

        diameter = GroundRadius(data.length, data.width) * 2;

        edge.transform.localScale = new Vector3(diameter, diameter);

        progress.transform.localScale = new Vector3(0, 0);

    }

    public override void setColor(Color color, Color stunning)
    {

        edge.GetComponent<SpriteRenderer>().color = stunning;
        progress.GetComponent<SpriteRenderer>().color = color;
    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = diameter * percent;
        progress.transform.localScale = new Vector3(length_percent, length_percent);
    }

    protected override float getThreat()
    {
        return data.powerByStrength / GlobalPlayer.gPlay.localPowerThreat;
    }

    protected override bool willStagger()
    {
        return data.stagger >= GlobalPlayer.gPlay.localStunThreat;
    }
}
