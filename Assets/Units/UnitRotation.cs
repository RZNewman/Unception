using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRotation : MonoBehaviour
{
    UnitMovement movement;
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponentInParent<UnitMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.localRotation = Quaternion.AngleAxis(movement.currentLookAngle, Vector3.up);
    }
}
