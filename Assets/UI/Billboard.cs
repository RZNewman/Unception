using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    void Update()
    {
        if (Camera.main)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
