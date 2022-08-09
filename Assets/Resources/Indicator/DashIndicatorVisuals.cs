using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateDash;

public class DashIndicatorVisuals : IndicatorInstance
{
    [SyncVar]
    protected DashInstanceData data;

    public GameObject line;
    public GameObject tip;
    public GameObject progress;

    float length;
    float scale;
    public override void setColor(Color color)
    {
        line.GetComponent<SpriteRenderer>().color = color;
        tip.GetComponent<SpriteRenderer>().color = color;
        progress.GetComponent<SpriteRenderer>().color = color;
    }

    public void setPosition(DashInstanceData dash, float scale)
    {
        data = dash;
        this.scale = scale;
        reposition();
    }

    protected override void reposition()
    {
        DashInstanceData dash = (DashInstanceData)data;

        length = dash.distance;
        float width = 0.1f * scale;

        line.transform.localScale = new Vector3(width, length);
        line.transform.localPosition = new Vector3(0, length / 2);

        Vector3 arrowScale = new Vector3(0.1f, 0.04f) * scale;
        tip.transform.localScale = arrowScale;
        tip.transform.localPosition = new Vector3(0, length);

        progress.transform.localScale = arrowScale * 2;

    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = length * percent;
        progress.transform.localPosition = new Vector3(0, length_percent);
    }
}
