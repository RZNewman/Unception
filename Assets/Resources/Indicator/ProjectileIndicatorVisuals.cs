using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;

public class ProjectileIndicatorVisuals : IndicatorInstance
{
    //TODO RZN Client sync on state
    protected ActionState state;


    public GameObject shot;
    public GameObject progress;

    float length;
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

        length = hit.length * 0.5f;
        width = hit.width;

        shot.transform.localScale = new Vector3(width, length);
        shot.transform.localPosition = new Vector3(0, length / 2);

        progress.transform.localScale = new Vector3(width, 0);

    }

    public override void setColor(Color color)
    {
        shot.GetComponent<SpriteRenderer>().color = color;
        progress.GetComponent<SpriteRenderer>().color = color;
    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = length * percent;
        progress.transform.localScale = new Vector3(progress.transform.localScale.x, length_percent);
        progress.transform.localPosition = new Vector3(0, length_percent / 2);
    }

    protected override float getThreat()
    {
        return state.getSource().powerByStrength / FindObjectOfType<GlobalPlayer>().localPower;
    }
}
