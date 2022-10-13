using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;
using static UnitControl;
using static Utils;

public static class AttackUtils
{
    public struct KnockBackVectors
    {
        public Vector3 center;
        public Vector3 direction;
    }
    public static void hit(GameObject other, UnitMovement mover, HitInstanceData hitData, uint team, float power, KnockBackVectors knockbackData)
    {
        if (other.GetComponentInParent<TeamOwnership>().getTeam() != team)
        {

            if (mover)
            {
                other.GetComponentInParent<LifeManager>().getHit(mover.gameObject);
            }
            Health h = other.GetComponentInParent<Health>();
            if (h)
            {
                h.takeDamage(hitData.damageMult * Power.damageFalloff(hitData.powerAtGen, power));
            }
            Posture p = other.GetComponentInParent<Posture>();
            if (p)
            {
                p.takeStagger(hitData.stagger);
            }
            UnitMovement otherMover = other.GetComponentInParent<UnitMovement>();
            if (otherMover)
            {
                Vector3 knockBackVec;
                switch (hitData.knockBackType)
                {
                    case KnockBackType.inDirection:
                        knockBackVec = hitData.knockback * knockbackData.direction;

                        break;
                    case KnockBackType.fromCenter:
                        Vector3 dir = other.transform.position - knockbackData.center;
                        dir.y = 0;
                        dir.Normalize();
                        knockBackVec = hitData.knockback * dir;
                        break;
                    default:
                        throw new System.Exception("No kb type");
                }
                switch (hitData.knockBackDirection)
                {
                    case KnockBackDirection.Backward:
                        knockBackVec *= -1;
                        break;
                }
                otherMover.applyForce(knockBackVec);
                otherMover.applyForce(hitData.knockUp * Vector3.up);
            }


        }
    }
    public struct AttackSegment
    {
        public List<AttackStageState> states;
        public WindState windup;
        public WindState winddown;
        public ActionState action;
        public GameObject groundTargetInstance;

        bool pastWindup;
        AttackStageState currentState;

        public void enterSegment(UnitMovement mover)
        {
            pastWindup = false;
            HitInstanceData source = action.getSource();
            if (source.type == HitType.Ground)
            {
                GameObject body = mover.getSpawnBody();
                Size s = body.GetComponentInChildren<Size>();
                //TODO Two ground target options, how to sync up?
                groundTargetInstance = SpawnGroundTarget(body.transform, s.scaledRadius, s.scaledHalfHeight, mover.lookWorldPos, source.length, mover.isServer);
                groundTargetInstance.GetComponent<GroundTarget>().height = s.indicatorHeight;
                windup.setGroundTarget(groundTargetInstance, new FloorNormal.GroundSearchParams
                {
                    radius = s.scaledRadius,
                    distance = s.scaledHalfHeight,
                });
                action.setGroundTarget(groundTargetInstance);
            }
        }
        public void exitSegment()
        {
            if (groundTargetInstance)
            {
                GameObject.Destroy(groundTargetInstance);
            }
        }
        public AttackStageState nextState()
        {
            if (states.Count == 0)
            {
                return null;
            }
            if (currentState == windup)
            {
                pastWindup = true;
            }
            currentState = states[0];
            states.RemoveAt(0);
            return currentState;
        }

        public float remainingWindDown()
        {
            if (pastWindup)
            {
                return winddown.remainingDuration;
            }
            else
            {
                return 0;
            }
        }
    }


    static GameObject SpawnGroundTarget(Transform body, float radius, float height, Vector3 target, float length, bool isServer)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().GroundTargetPre;
        Vector3 bodyFocus = body.position + body.forward * radius;
        Vector3 diff = target - bodyFocus;


        Vector3 forwardDiff = Mathf.Max(Vector3.Dot(diff, body.forward), 0) * body.forward;
        forwardDiff.y = diff.y;
        Vector3 limitedDiff = forwardDiff.normalized * Mathf.Min(length, forwardDiff.magnitude);

        //float angleOff = Vector3.SignedAngle(body.forward, planarDiff, Vector3.up);
        //Vector3 forwardLine = Quaternion.AngleAxis(angleOff, Vector3.up) * diff;
        //Vector3 offset = forwardLine.normalized * Mathf.Min(length, Vector3.Dot(diff, forwardLine));


        GameObject instance = GameObject.Instantiate(prefab, bodyFocus + limitedDiff, body.rotation);
        if (isServer)
        {
            NetworkServer.Spawn(instance);
        }

        return instance;
    }



    public static void SpawnProjectile(FloorNormal floor, Transform body, float radius, float halfHeight, UnitMovement mover, HitInstanceData hitData)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().ProjectilePre;
        Vector3 groundFocus = body.position + body.forward * radius + Vector3.down * halfHeight;
        Vector3 bodyFocus = groundFocus + floor.normal * halfHeight;
        Quaternion aim = floor.getAimRotation(body.forward);
        GameObject instance = GameObject.Instantiate(prefab, bodyFocus, aim);
        Projectile p = instance.GetComponent<Projectile>();
        float hitRadius = hitData.width / 2;
        float terrainRadius = Mathf.Min(hitRadius, halfHeight * 0.5f);
        p.init(terrainRadius, hitRadius, halfHeight, mover, hitData);
        NetworkServer.Spawn(instance);
    }
    public static List<GameObject> LineAttack(FloorNormal floor, Transform body, float radius, float halfHeight, float length, float width)
    {
        float playerHeightOversize = halfHeight * 2 * 1.5f;
        List<GameObject> hits = new List<GameObject>();
        List<GameObject> tempHits = new List<GameObject>();
        Vector3 groundFocus = body.position + body.forward * radius + Vector3.down * halfHeight;
        Vector3 bodyFocus = groundFocus + floor.normal * halfHeight;
        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        Quaternion aim = floor.getAimRotation(body.forward);
        Vector3 boxCenter = bodyFocus + maxDistance * 0.5f * (aim * Vector3.forward);
        float boxHeight = Mathf.Max(playerHeightOversize, maxDistance);
        Vector3 boxHalfs = new Vector3(width / 2, boxHeight / 2, maxDistance / 2);

        RaycastHit[] boxHits = Physics.BoxCastAll(boxCenter, boxHalfs, body.forward, aim, 0.0f, LayerMask.GetMask("Players", "Breakables"));
        //RaycastHit[] sphereHits = Physics.SphereCastAll(bodyFocus, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players"));
        float capsuleHeightFactor = Mathf.Max(boxHeight / 2 - maxDistance, 0);
        Vector3 capsuleHeightDiff = floor.normal * capsuleHeightFactor;
        Vector3 capsuleStart = bodyFocus + capsuleHeightDiff;
        Vector3 capsuleEnd = bodyFocus - capsuleHeightDiff;
        RaycastHit[] capsuleHits = Physics.CapsuleCastAll(capsuleStart, capsuleEnd, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players", "Breakables"));

        //Debug.DrawLine(bodyFocus, bodyFocus + body.forward * maxDistance, Color.blue, 3.0f); ;
        //Debug.DrawLine(bodyFocus, bodyFocus + (body.forward+body.up).normalized * maxDistance, Color.blue, 3.0f);
        //DrawBox(boxCenter, aim, boxHalfs * 2, Color.blue);
        //Debug.DrawLine(capsuleStart, capsuleEnd, Color.red);
        //Debug.DrawLine(capsuleStart, capsuleStart+ body.forward*maxDistance, Color.red);
        //Debug.DrawLine(capsuleEnd, capsuleEnd + body.forward * maxDistance, Color.red);
        //Debug.Break();

        foreach (RaycastHit hit in boxHits)
        {
            GameObject obj = hit.collider.gameObject;
            tempHits.Add(obj);
        }
        foreach (RaycastHit hit in capsuleHits)
        {
            GameObject obj = hit.collider.gameObject;
            if (tempHits.Contains(obj))
            {
                hits.Add(obj);
            }
        }

        return hits;

    }
    public static List<GameObject> GroundAttack(Vector3 origin, float radius)
    {
        List<GameObject> hits = new List<GameObject>();

        RaycastHit[] sphereHits = Physics.SphereCastAll(origin, radius, Vector3.forward, 0.0f, LayerMask.GetMask("Players", "Breakables"));



        foreach (RaycastHit hit in sphereHits)
        {
            GameObject obj = hit.collider.gameObject;
            hits.Add(obj);
        }


        return hits;

    }
}
