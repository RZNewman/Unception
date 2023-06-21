using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;

public class GroundIndicatorVisuals : HitIndicatorInstance
{


    public GameObject edge;
    public GameObject progress;

    float width;
    // Start is called before the first frame update

    protected override void setSize()
    {

        width = data.width + data.length;

        edge.transform.localScale = new Vector3(width, width);

        progress.transform.localScale = new Vector3(0, 0);

    }

    public override void setColor(Color color, Color stunning)
    {

        edge.GetComponent<SpriteRenderer>().color = stunning;
        progress.GetComponent<SpriteRenderer>().color = color;
    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = width * percent;
        progress.transform.localScale = new Vector3(length_percent, length_percent);
    }

    protected override float getThreat()
    {
        return data.powerByStrength / FindObjectOfType<GlobalPlayer>().localPowerThreat;
    }

    protected override bool willStagger()
    {
        return data.stagger >= FindObjectOfType<GlobalPlayer>().localStunThreat;
    }
}
