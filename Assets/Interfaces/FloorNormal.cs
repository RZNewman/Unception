using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorNormal : MonoBehaviour
{
    bool ground = false;
    Vector3 groundNormal;

    public struct GroundSearchParams
    {
        public float radius;
        public float distance;
    }
    public void setGround(GroundSearchParams paras)
    {

        RaycastHit rout;

        bool terrain = Physics.SphereCast(transform.position, paras.radius, -transform.up, out rout, paras.distance * 1.01f, LayerMask.GetMask("Terrain"));
        float angle = Vector3.Angle(Vector3.up, rout.normal);

        ground = terrain && angle < 45;

        if (ground)
        {
            groundNormal = rout.normal;
        }
        else
        {
            groundNormal = Vector3.up;
        }


    }
    public Vector3 normal
    {
        get
        {
            return groundNormal;
        }
    }

    public Quaternion getIndicatorRotation(Vector3 forward)
    {
        return Quaternion.LookRotation(groundNormal, forward);
    }

    public Quaternion getAimRotation(Vector3 forward)
    {
        Vector3 left = Vector3.Cross(forward, groundNormal);
        Vector3 aim = Vector3.Cross(groundNormal, left);
        return Quaternion.LookRotation(aim, groundNormal);
    }

    public bool hasGround
    {
        get
        {
            return ground;
        }
    }
}
