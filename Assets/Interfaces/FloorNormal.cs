using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using static Size;

public class FloorNormal : MonoBehaviour
{
    bool ground = false;
    Vector3 groundNormal = Vector3.up;
    Vector3 navPosition = Vector3.zero;

    public static readonly float floorDegrees = 45;


    Vector3 cachedPosition = Vector3.zero;
    public void setGround(CapsuleSize sizeC)
    {
        Vector3 diff = transform.position - cachedPosition;
        if (diff.magnitude < sizeC.radius * 0.2f)
        {
            return;
        }

        GroundResult calc = getGroundNormal(transform.position, sizeC);

        ground = calc.ground;
        groundNormal = calc.normal;

        //NNInfo nodeInfo = AstarPath.active.GetNearest(transform.position);
        //if (NavMesh.SamplePosition(transform.position, out hit, sizeC.distance * 3, NavMesh.AllAreas))
        RaycastHit hit;
        //if(nodeInfo.node != null & (nodeInfo.position - transform.position).magnitude < sizeC.distance * 3)
        if(Physics.Raycast(transform.position,Vector3.down,out hit,sizeC.distance*3,LayerMask.GetMask("Terrain")))
        {
            navPosition = hit.point;
        }

        cachedPosition = transform.position;
    }

    public struct GroundResult
    {
        public bool ground;
        public Vector3 normal;
    }

    public static GroundResult getGroundNormal(Vector3 position, CapsuleSize sizeC)
    {
        RaycastHit rout;
        bool terrain = Physics.SphereCast(position + Vector3.up * sizeC.distance, sizeC.radius * 0.95f, Vector3.down, out rout, sizeC.distance * 2.01f, LayerMask.GetMask("Terrain"));
        float angle = Vector3.Angle(Vector3.up, rout.normal);

        bool ground = terrain && angle < floorDegrees;

        Vector3 groundNormal;
        if (ground)
        {
            groundNormal = rout.normal;
        }
        else
        {
            groundNormal = Vector3.up;
        }
        return new GroundResult
        {
            ground = ground,
            normal = groundNormal,
        };
    }
    public Vector3 normal
    {
        get
        {
            return groundNormal;
        }
    }

    public Vector3 nav
    {
        get
        {
            return navPosition;
        }
    }

    public Vector3 forwardPlanarWorld(Vector3 forward)
    {
        //Vector3 left = Vector3.Cross(forward, groundNormal);
        //Vector3 aim = Vector3.Cross(groundNormal, left);
        //return aim;
        return Vector3.ProjectOnPlane(forward, groundNormal).normalized;
    }

    public Quaternion getIndicatorRotation(Vector3 forward)
    {
        //backwards, bc indicators are build backwards
        return Quaternion.LookRotation(groundNormal, forward);
    }

    public Quaternion getAimRotation(Vector3 forward)
    {
        //Debug.Log(forwardPlanarWorld(forward));
        return Quaternion.LookRotation(forwardPlanarWorld(forward), groundNormal);
    }

    public bool hasGround
    {
        get
        {
            return ground;
        }
    }
}
