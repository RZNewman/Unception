using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Size : MonoBehaviour
{
    CapsuleCollider col;
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider>();   
    }

    public float indicatorHeight
    {
        get {
            return (col.height / 2) - 0.01f;
        }
    }
    public float indicatorForward
    {
        get
        {
            return col.radius;
        }
    }
}
