using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateDash;

public class DashIndicatorVisuals : IndicatorInstance
{
    protected DashState state;

    public GameObject line;
    public GameObject tip;
    public GameObject progress;

    float length;
    float scale;
    public override void setColor(Color color, Color stunning)
    {
        line.GetComponent<SpriteRenderer>().color = color;
        tip.GetComponent<SpriteRenderer>().color = color;
        progress.GetComponent<SpriteRenderer>().color = color;
    }

    public void setSource(DashState dash, Power p)
    {
        state = dash;
        scale = p.scale();
        setSize();
    }

    protected override void setSize()
    {
        DashInstanceData dash = state.getSource();

        

        length = dash.distance;
        float width = 0.1f * scale;
        Quaternion rotation = Quaternion.identity;
        if (dash.control == DashControl.Backward)
        {
            rotation = Quaternion.AngleAxis(180, Vector3.forward);
            length *= -1;
        }

        line.transform.localScale = new Vector3(width, Mathf.Abs(length));
        line.transform.localPosition = new Vector3(0, length / 2);
        line.transform.localRotation = rotation;

        Vector3 arrowScale = new Vector3(0.5f, 0.2f) * scale;
        tip.transform.localScale = arrowScale;
        tip.transform.localPosition = new Vector3(0, length);
        tip.transform.localRotation = rotation;

        progress.transform.localScale = arrowScale * 2;
        progress.transform.localRotation = rotation;

    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = length * percent;
        progress.transform.localPosition = new Vector3(0, length_percent);
    }
}
