using Mirror;
using UnityEngine;
using static GenerateHit;

public class LineIndicatorVisuals : IndicatorInstance
{
    protected ActionState state;


    public GameObject mask;
    public GameObject square;
    public GameObject circle;
    public GameObject progress;

    float length;
    float width;
    float range;
    // Start is called before the first frame update

    public void setSource(ActionState s)
    {
        state = s;
        setSize();
    }
    protected override void setSize()
    {
        HitInstanceData hit = state.getSource();

        length = hit.length;
        width = hit.width;
        range = hit.range;

        square.transform.localScale = new Vector3(width, length);
        square.transform.localPosition = new Vector3(0, range + length / 2);

        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        float sphereScale = maxDistance * 2;

        circle.transform.localScale = new Vector3(sphereScale, sphereScale);
        circle.transform.localPosition = new Vector3(0, range);

        mask.transform.localScale = new Vector3(sphereScale, sphereScale);
        mask.transform.localPosition = new Vector3(0, range + length - maxDistance);

        progress.transform.localScale = new Vector3(width, 0);
        progress.transform.localPosition = new Vector3(0, range);

    }

    public override void setColor(Color color, Color stunning)
    {

        square.GetComponent<SpriteRenderer>().color = stunning;
        circle.GetComponent<SpriteRenderer>().color = stunning;

        progress.GetComponent<SpriteRenderer>().color = color;
    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = length * percent;
        progress.transform.localScale = new Vector3(progress.transform.localScale.x, length_percent);
        progress.transform.localPosition = new Vector3(0, range + length_percent / 2);
    }

    protected override float getThreat()
    {
        return state.getSource().powerByStrength / FindObjectOfType<GlobalPlayer>().localPowerThreat;
    }

    protected override bool willStagger()
    {
        return state.getSource().stagger >= FindObjectOfType<GlobalPlayer>().localStunThreat;
    }
}
