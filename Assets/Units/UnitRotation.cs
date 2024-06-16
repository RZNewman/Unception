using UnityEngine;

public class UnitRotation : MonoBehaviour
{
    UnitMovement movement;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        updateRotation();

    }

    public void updateRotation()
    {
        if (!movement)
        {
            movement = GetComponentInParent<UnitMovement>();
        }
        transform.localRotation = Quaternion.AngleAxis(movement.currentLookAngle, Vector3.up);
    }

}
