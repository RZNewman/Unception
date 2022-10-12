using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IndicatorHolder;

public class GroundTarget : NetworkBehaviour, IndicatorHolder
{
    [HideInInspector]
    public float height;

    [SyncVar]
    public Vector3 target;
    [SyncVar]
    public float speed;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public Vector3 indicatorPosition(Vector3 forward)
    {
        return Vector3.down * height;
    }
    public float offsetMultiplier()
    {
        return 0.0f;
    }
    public IndicatorLocalPoint pointOverride(Vector3 fowardPlanar, Vector3 groundNormal)
    {
        return new IndicatorLocalPoint
        {
            shouldOverride = false,
        };
    }

    public void setTarget(Vector3 t, float s)
    {
        t.y = transform.position.y;
        target = t;
        speed = s;
        moveTowardTarget();
    }

    public void moveTowardTarget()
    {

        Vector3 diff = target - transform.position;
        float frameDistance = speed * Time.fixedDeltaTime;
        if (diff.magnitude < frameDistance)
        {
            transform.position = target;
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.velocity = speed * diff.normalized;
        }


    }


}
