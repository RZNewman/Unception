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
        if (movement.isServer)
        {
            transform.localRotation = Quaternion.AngleAxis(movement.currentLookAngle, Vector3.up);
        }
    }

}
