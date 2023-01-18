using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public void setDirection(Vector3 dir)
    {
        //Debug.Log(dir);
        transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(dir, Vector3.forward, Vector3.up), Vector3.forward);
    }

}
