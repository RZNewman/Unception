using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientAdoption : NetworkBehaviour 
{
    [HideInInspector]
    [SyncVar]
    public GameObject parent;

    // Start is called before the first frame update
    void Start()
    {
        if (isClientOnly)
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            transform.parent = parent.transform;
            transform.localPosition = position;
            transform.localRotation = rotation;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
