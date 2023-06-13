using Mirror;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    // Start is called before the first frame
    public float gravity = -9.81f;
    UnitMovement movement;
    Rigidbody rb;
    LifeManager lifeManager;
    Power power;
    void Start()
    {
        movement = GetComponent<UnitMovement>();
        rb = GetComponent<Rigidbody>();
        lifeManager = GetComponent<LifeManager>();
        power = GetComponent<Power>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (movement)
        {
            //is unit
            if (!movement.grounded && !lifeManager.IsDead)
            {
                float frameGrav = gravity * Time.fixedDeltaTime * power.scaleSpeed();
                if (!movement.canFall())
                {
                    if (rb.velocity.y > 0)
                    {
                        if (rb.velocity.y + frameGrav < 0)
                        {
                            frameGrav = -rb.velocity.y;
                        }
                    }
                    else
                    {
                        frameGrav = 0;
                    }

                }
                rb.velocity += new Vector3(0, frameGrav, 0);

            }
        }
        else
        {
            rb.velocity += new Vector3(0, gravity, 0) * Time.fixedDeltaTime;
        }

    }
}
