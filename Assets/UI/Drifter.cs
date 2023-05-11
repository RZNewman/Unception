using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drifter : MonoBehaviour
{
    public Vector3 startingVelocity;

    public float lifetime = 2f;
    public float discount = 0.8f;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = startingVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity *= discount;
        lifetime -= Time.deltaTime;
        if(lifetime < 0)
        {
            Destroy(gameObject);
        }
    }
}
