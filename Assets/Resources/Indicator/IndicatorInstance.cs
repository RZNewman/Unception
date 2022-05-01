using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IndicatorInstance : MonoBehaviour
{
    public float maxTime;
    public float currentTime;

    uint teamOwner;
    public abstract void reposition(AttackData data);
    protected abstract void setCurrentProgress(float percent);

    public abstract void setColor(Color color);
    public virtual void setTime(float maxTime)
    {
        this.maxTime = maxTime;
        currentTime = maxTime;
    }

    protected void Update()
    {
        currentTime -= Time.deltaTime;
        setCurrentProgress(Mathf.Max(maxTime - currentTime, 0) / maxTime);

        setLocalPosition();
    }

    void setLocalPosition()
    {
        if (trackingBody)
        {
            UnitMovement master = trackingBody.GetComponentInParent<UnitMovement>();
            Size s = trackingBody.GetComponentInChildren<Size>();
            Quaternion q = Quaternion.LookRotation(-master.floorNormal, trackingBody.transform.forward);
            transform.rotation = q;

            Vector3 projection = Vector3.ProjectOnPlane(trackingBody.transform.forward, master.floorNormal).normalized;

            Vector3 forward = Vector3.forward;
            forward.y = projection.y;
            forward.Normalize();

            transform.localPosition = s.indicatorHeight * Vector3.down
                + s.indicatorForward * forward;

            //TODO indcator width should change based on the slope
        }
    }
    GameObject trackingBody;
    public void setTrackingBody(GameObject track)
    {
        trackingBody = track;
        setLocalPosition();
    }
    public void setTeam(uint team)
    {
        teamOwner = team;
        updateColor();
    }

    void updateColor()
    {
        if(teamOwner == 1u)
        {
            setColor(GameColors.FriendIndicator);
        }
        else
        {
            setColor(GameColors.EnemyIndicator);
        }
    }
}
