using UnityEngine;

public class Size : MonoBehaviour
{
    CapsuleCollider col;
    Vector3 baseSize;
    CapsuleCollider stopper;
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider>();
        stopper = transform.parent.GetComponentInChildren<UnitStopper>().GetComponent<CapsuleCollider>();
        Physics.IgnoreCollision(col, stopper);
        stopper.transform.parent = transform;
        stopper.transform.localScale = Vector3.one;
        stopper.radius = colliderWidth + 0.1f;
        stopper.height = colliderHalfHeight * 2 + 0.1f;
        GetComponentInParent<Power>().subscribePower(updateSize);
    }

    void updateSize(Power p)
    {
        transform.localScale = baseSize * p.scale();
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
        get
        {
            return scaledHalfHeight * 0.99f;
        }
    }

    public float scaledHalfHeight
    {
        get
        {
            return colliderHalfHeight * transform.lossyScale.y;
        }
    }

    float colliderHalfHeight
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
    public float scaledRadius
    {
        get
        {

            return colliderWidth * transform.lossyScale.z;
        }
    }
}
