using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropScaler : MonoBehaviour
{
    public Vector3 min = Vector3.one;
    public Vector3 max = Vector3.one;
    public void scale(Vector3 s)
    {
        transform.localScale = s;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
