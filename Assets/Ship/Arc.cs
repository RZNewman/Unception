using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arc : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Action<Vector3> result;

    public void init(Vector3 dir, float mag,  Action<Vector3> callback)
    {
        result = callback;
        GetComponent<Rigidbody>().velocity = dir *mag;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isServer)
        {
            result(transform.position);
            Destroy(gameObject);
        }
        
    }
}
