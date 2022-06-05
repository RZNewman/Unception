using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Size : MonoBehaviour
{
    CapsuleCollider col;
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider>();
        Collider stopper = transform.parent.GetComponentInChildren<UnitStopper>().GetComponent<Collider>();
        Physics.IgnoreCollision(col, stopper);
    }
    public Collider coll
    {
        get { return col; }
    }

    public float indicatorHeight
    {
        get {
            float dist;
            if(col.direction == 2)
            {
                dist = col.radius - 0.01f;
            }
            else
            {
                dist = (col.height / 2) - 0.01f;
            }
            return dist * transform.lossyScale.y;
        }
    }
    public float indicatorForward
    {
        get
        {
            float dist;
            if (col.direction == 2)
            {
                dist = col.height / 2;
            }
            else
            {
                dist = col.radius;
            }
            return dist * transform.lossyScale.z;
        }
    }
}
