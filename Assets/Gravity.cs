using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    // Start is called before the first frame
    public float gravity = -9.81f;
    UnitMovement movement;
    Rigidbody rb;
    void Start()
    {
        movement = GetComponent<UnitMovement>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!movement.grounded)
		{
            rb.velocity += new Vector3(0, gravity, 0) *Time.fixedDeltaTime;
		}
    }
}
