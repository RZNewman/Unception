using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;

public class GroundIndicatorVisuals : IndicatorInstance
{
    //TODO RZN Client sync on state
    protected ActionState state;


    public GameObject edge;
    public GameObject progress;

    float width;
    // Start is called before the first frame update

    public void setSource(ActionState s)
    {
        state = s;
        reposition();
    }
    protected override void reposition()
    {
        HitInstanceData hit = state.getSource();

        width = hit.width;

        edge.transform.localScale = new Vector3(width, width);

        progress.transform.localScale = new Vector3(0, 0);

    }

    public override void setColor(Color color)
    {
        edge.GetComponent<SpriteRenderer>().color = color;
        progress.GetComponent<SpriteRenderer>().color = color;
    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = width * percent;
        progress.transform.localScale = new Vector3(length_percent, length_percent);
    }

    protected override float getThreat()
    {
        return state.getSource().powerByStrength / FindObjectOfType<GlobalPlayer>().localPower;
    }
}
