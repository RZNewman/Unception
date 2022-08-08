using Mirror;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;

public abstract class IndicatorInstance : NetworkBehaviour
{
    [SyncVar]
    public float maxTime;
    [SyncVar]
    public float currentTime;

    [SyncVar]
    uint teamOwner;

    [SyncVar]
    protected HitInstanceData data;
    public void reposition(HitInstanceData aData)
    {
        data = aData;
        repositionImpl();
    }

    protected abstract void repositionImpl();
    protected abstract void setCurrentProgress(float percent);

    public abstract void setColor(Color color);
    public virtual void setTime(float maxTime)
    {
        this.maxTime = maxTime;
        currentTime = maxTime;
    }
    private void Start()
    {
        if (isClientOnly)
        {
            repositionImpl();
            updateColor();
        }
    }

    protected void Update()
    {
        currentTime -= Time.deltaTime;
        setCurrentProgress(Mathf.Max(maxTime - currentTime, 0) / maxTime);

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
                + s.scaledRadius * forward;

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
