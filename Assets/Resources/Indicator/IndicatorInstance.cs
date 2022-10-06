using Mirror;
using UnityEngine;
using static Cast;
using static GenerateAttack;
using static GenerateHit;

public abstract class IndicatorInstance : MonoBehaviour
{
    public float maxTime = 0;

    public IndicatorOffsets currentOffsets;


    uint teamOwner;


    protected abstract void reposition();
    protected abstract void setCurrentProgress(float percent);

    public abstract void setColor(Color color);

    public void setLocalOffsets(IndicatorOffsets offsets)
    {
        currentOffsets = offsets;
        if (offsets.time > maxTime)
        {
            maxTime = offsets.time;
        }
    }
    private void Start()
    {
    }

    public void OrderedUpdate()
    {
        float progress;
        if (maxTime == 0)
        {
            progress = 0;
        }
        else
        {
            progress = Mathf.Max(maxTime - currentOffsets.time, 0) / maxTime;
        }

        updateColor();
        setCurrentProgress(progress);
        setLocalPosition();
    }

    void setLocalPosition()
    {
        if (transform.parent)
        {
            GameObject trackingBody = transform.parent.gameObject;
            FloorNormal ground = trackingBody.GetComponentInParent<FloorNormal>();
            IndicatorHolder ih = trackingBody.GetComponentInChildren<IndicatorHolder>();
            transform.rotation = ground.getIndicatorRotation(trackingBody.transform.forward);

            Vector3 projection = Vector3.ProjectOnPlane(trackingBody.transform.forward, ground.normal).normalized;

            Vector3 forward = Vector3.forward;
            forward.y = projection.y;
            forward.Normalize();

            transform.localPosition = ih.indicatorPosition(forward) + currentOffsets.distance * ih.offsetMultiplier();

        }
    }

    public void setTeam(uint team)
    {
        teamOwner = team;
        updateColor();
    }

    void updateColor()
    {
        float threat = getThreat();
        setColor(getIndicatorColor(teamOwner, threat));
    }

    public static Color getIndicatorColor(uint team, float threat)
    {
        if (team == 1u)
        {
            return GameColors.FriendIndicator;
        }
        else
        {



            return getEnemyThreatColor(threat);
        }
    }

    public static Color getEnemyThreatColor(float threat)
    {
        Color middle = GameColors.EnemyIndicator;
        Color low = middle;
        low.a = 0.1f;
        Color high = GameColors.EnemyIndicatorHigh;

        Color output;
        if (threat <= 1)
        {
            output = Color.Lerp(low, middle, threat);
        }
        else
        {
            output = Color.Lerp(middle, high, (threat - 1) / 3.0f);
        }
        return output;

    }

    protected virtual float getThreat()
    {
        return 1.0f;
    }
}
