using Mirror;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    // Start is called before the first frame
    public float gravity = -9.81f;
    UnitMovement movement;
    Rigidbody rb;
    LifeManager lifeManager;
    ModelLoader model;
    Power power;
    void Start()
    {
        movement = GetComponent<UnitMovement>();
        rb = GetComponent<Rigidbody>();
        lifeManager = GetComponent<LifeManager>();
        model = GetComponent<ModelLoader>();
        power = GetComponent<Power>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (movement)
        {
            //is unit
            if (!movement.grounded && !lifeManager.IsDead && model.modelLoaded)
            {
                rb.velocity += new Vector3(0, gravity, 0) * Time.fixedDeltaTime * power.scale();
            }
        }
        else
        {
            rb.velocity += new Vector3(0, gravity, 0) * Time.fixedDeltaTime;
        }

    }
}
