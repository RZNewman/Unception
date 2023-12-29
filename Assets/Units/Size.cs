using UnityEngine;
using static IndicatorHolder;

public class Size : MonoBehaviour
{
    CapsuleCollider col;
    Vector3 baseSize = Vector3.one;
    CapsuleCollider stopper;
    // Start is called before the first frame update

    private void Awake()
    {
        col = GetComponent<CapsuleCollider>();
    }

    public struct CapsuleSize
    {
        public float radius;
        public float distance;
        public float indicatorHeight
        {
            get
            {
                return distance * 0.45f;
            }
        }
        public Vector3 indicatorPosition(Vector3 worldForward)
        {
            float worldAngle = Vector2.SignedAngle(Vector2.up, new Vector2(worldForward.x, worldForward.z));
            Quaternion worldToLocal = Quaternion.AngleAxis(worldAngle, Vector3.up);
            return indicatorHeight * Vector3.down
                  + radius * (worldToLocal * worldForward);
        }
    }

    public CapsuleSize sizeC
    {
        get
        {
            return new CapsuleSize
            {
                distance = scaledHalfHeight,
                radius = scaledRadius,
            };
        }

    }

    void Start()
    {
        stopper = transform.parent.GetComponentInChildren<UnitStopper>().GetComponent<CapsuleCollider>();
        Physics.IgnoreCollision(col, stopper);
        stopper.transform.parent = transform;
        stopper.transform.localScale = Vector3.one;
        stopper.radius = colliderWidth + 0.01f;
        stopper.height = colliderHalfHeight * 2 + 0.01f;
        GetComponentInParent<Power>().subscribePower(updateSize);
    }

    Vector3 cachedSize = Vector3.one;
    void updateSize(Power p)
    {
        cachedSize = baseSize * p.scalePhysical();
        transform.localScale = cachedSize;
    }

    public void setBaseSize(Vector3 size)
    {
        baseSize = size;
    }





    public float scaledHalfHeight
    {
        get
        {
            return colliderHalfHeight * cachedSize.y;
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

            return colliderWidth * cachedSize.z;
        }
    }
}
