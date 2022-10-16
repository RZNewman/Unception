using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiSpinner : MonoBehaviour
{
    public float rotationSpeed = 45;
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.forward)* transform.rotation;
    }
}
