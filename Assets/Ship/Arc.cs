using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arc : NetworkBehaviour
{
    bool called = false;
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
        if (isServer && ! called)
        {
            called = true;
            result(transform.position);
            Destroy(gameObject);
        }
        
    }
}
