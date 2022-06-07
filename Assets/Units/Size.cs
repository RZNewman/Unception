using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Size : MonoBehaviour
{
    CapsuleCollider col;
    Vector3 baseSize;
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider>();
        transform.localScale = baseSize * GetComponentInParent<Power>().downscaled/Power.baseDownscale;
        CapsuleCollider stopper = transform.parent.GetComponentInChildren<UnitStopper>().GetComponent<CapsuleCollider>();
        Physics.IgnoreCollision(col, stopper);
        stopper.transform.parent = transform;
        stopper.transform.localScale = Vector3.one;
        stopper.radius = colliderWidth + 0.1f;
        stopper.height = colliderHeight * 2 + 0.1f;
    }

    public void setBaseSize(Vector3 size)
    {
        baseSize = size;
    }

    public Collider coll
    {
        get { return col; }
    }

    public float indicatorHeight
    {
        get {
            float dist = colliderHeight - 0.01f;
    
            return dist * transform.lossyScale.y;
        }
    }

    float colliderHeight
    {
        get
        {
            if (col.direction == 2)
            {
                return col.radius;
            }
            else
            {
                return col.height / 2;
            }
        }
    }
    float colliderWidth
    {
        get
        {
            if (col.direction == 2)
            {
                return col.height / 2;
            }
            else
            {
                return col.radius;
            }
        }
    }
    public float indicatorForward
    {
        get
        {
            
            return colliderWidth * transform.lossyScale.z;
        }
    }
}
