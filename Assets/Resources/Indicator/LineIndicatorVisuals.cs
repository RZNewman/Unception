using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineIndicatorVisuals : IndicatorInstance
{
    public GameObject mask;
    public GameObject square;
    public GameObject circle;
    public GameObject progress;

    float length;
    float width;
    // Start is called before the first frame update
    public override void reposition(AttackData data)
    {
        length = data.length;
        width = data.width;

        square.transform.localScale = new Vector3(width, length);
        square.transform.localPosition = new Vector3(0, length / 2);

        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        float sphereScale = maxDistance * 2;

        circle.transform.localScale = new Vector3(sphereScale, sphereScale);
        circle.transform.localPosition = Vector3.zero;

        mask.transform.localScale = new Vector3(sphereScale, sphereScale);
        mask.transform.localPosition = new Vector3(0, length - maxDistance);

        progress.transform.localScale = new Vector3(width, 0);

    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = length * percent;
        progress.transform.localScale = new Vector3(progress.transform.localScale.x, length_percent);
        progress.transform.localPosition = new Vector3(0, length_percent / 2);
    }

}
