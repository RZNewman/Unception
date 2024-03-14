using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    Vector3 target;
    LocalCamera c;
    public void setTarget(Vector3 t)
    {
        target = t;
        //Debug.Log(dir);

    }
    private void Update()
    {
        if (!c)
        {
            c = FindObjectOfType<LocalCamera>();
        }
        if (c)
        {
            Vector3 diff = target - c.transform.parent.position;
            float angle = Vector3.SignedAngle(diff, Vector3.forward, Vector3.up);
            transform.localRotation = Quaternion.AngleAxis(angle + c.currentLookAngle, Vector3.forward);
        }
    }

}
