using Mirror;
using UnityEngine;
using static Cast;
using static GenerateAttack;
using static GenerateHit;

public abstract class IndicatorInstance : NetworkBehaviour
{
    [SyncVar]
    public float maxTime = 0;
    [SyncVar]
    public IndicatorOffsets currentOffsets;

    [SyncVar]
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
        if (isClientOnly)
        {
            reposition();
            updateColor();
        }
    }

    public void ServerUpdate()
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

        setCurrentProgress(progress);

        setLocalPosition();
    }

    void setLocalPosition()
    {
        if (transform.parent)
        {
            GameObject trackingBody = transform.parent.gameObject;
            UnitMovement master = trackingBody.GetComponentInParent<UnitMovement>();
            Size s = trackingBody.GetComponentInChildren<Size>();
            Quaternion q = Quaternion.LookRotation(-master.floorNormal, trackingBody.transform.forward);
            transform.rotation = q;

            Vector3 projection = Vector3.ProjectOnPlane(trackingBody.transform.forward, master.floorNormal).normalized;

            Vector3 forward = Vector3.forward;
            forward.y = projection.y;
            forward.Normalize();

            transform.localPosition = s.indicatorHeight * Vector3.down
                + s.scaledRadius * forward + currentOffsets.distance;

            //TODO indcator width should change based on the slope
        }
    }

    public void setTeam(uint team)
    {
        teamOwner = team;
        updateColor();
    }

    void updateColor()
    {
        if (teamOwner == 1u)
        {
            setColor(GameColors.FriendIndicator);
        }
        else
        {
            setColor(GameColors.EnemyIndicator);
        }
    }
}
