using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public float storedAngle = 0;
    LocalCamera c;
    public void setDirection(Vector3 dir)
    {
        storedAngle = Vector3.SignedAngle(dir, Vector3.forward, Vector3.up);
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
            transform.rotation = Quaternion.AngleAxis(storedAngle + c.currentLookAngle, Vector3.forward);
        }
    }

}
