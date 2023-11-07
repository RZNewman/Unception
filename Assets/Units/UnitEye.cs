using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEye : MonoBehaviour
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

        updateRotation();

    }

    public void updateRotation()
    {
        transform.localRotation = Quaternion.AngleAxis(movement.currentLookVerticalAngle, Vector3.right);
    }
}
