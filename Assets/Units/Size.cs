using ShaderCrew.SeeThroughShader;
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
                return distance - IndicatorHeightOffset;
            }
        }
        public Vector3 indicatorPosition(Vector3 worldForward)
        {
            float worldAngle = Vector2.SignedAngle(Vector2.up, new Vector2(worldForward.x, worldForward.z));
            Quaternion worldToLocal = Quaternion.AngleAxis(worldAngle, Vector3.up);
            return indicatorHeight * Vector3.down
                  + radius * (worldToLocal * worldForward);
        }
        public IndicatorLocalLook pointOverride(Transform body, Vector3 fowardPlanar, Vector3 groundNormal)
        {
            Vector3 bodyFocus = body.position + radius * body.forward;
            Vector3 farDiff = radius * 5f * fowardPlanar;
            Vector3 heightDiff = groundNormal * indicatorHeight * 2;

            Vector3 indicatorLocalPos = indicatorPosition(fowardPlanar);
            Vector3 indicatorWorldPos = body.position + indicatorLocalPos;

            Vector3 indicatorPoint = indicatorWorldPos + farDiff;
            Vector3 castPoint = indicatorPoint + heightDiff;

            Vector3 lookDiff = indicatorPoint - bodyFocus;
            if (Physics.Raycast(bodyFocus, lookDiff, (lookDiff).magnitude, LayerMask.GetMask("Terrain")))
            {
                RaycastHit info;
                Vector3 localPoint;
                if (Physics.Raycast(castPoint, -groundNormal, out info, heightDiff.magnitude, LayerMask.GetMask("Terrain")))
                {
                    localPoint = info.point;
                }
                else
                {
                    localPoint = castPoint;
                }
                return new IndicatorLocalLook
                {
                    shouldOverride = true,
                    newForward = localPoint - body.position - indicatorLocalPos,
                };
            }
            else
            {
                return new IndicatorLocalLook
                {
                    shouldOverride = false,
                };
            }

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

        if (GetComponentInParent<UnitPropsHolder>().props.isPlayer)
        {
            FindObjectOfType<PlayersPositionManager>().AddPlayerAtRuntime(stopper.gameObject);
            stopper.GetComponent<SeeThroughShaderPlayer>().enabled = true;
        }
        GetComponentInParent<Power>().subscribePower(updateSize);
    }

    void updateSize(Power p)
    {
        transform.localScale = baseSize * p.scalePhysical();
    }

    public void setBaseSize(Vector3 size)
    {
        baseSize = size;
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
