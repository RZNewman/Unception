using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;
using static UnitControl;

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
            Health h = other.GetComponentInParent<Health>();
            if (mover)
            {
                other.GetComponentInParent<Combat>().getHit(mover.gameObject);
            }
            h.takeDamage(hitData.damageMult * power);
            other.GetComponentInParent<Posture>().takeStagger(hitData.stagger);
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
            other.GetComponentInParent<UnitMovement>().applyForce(knockBackVec);
            other.GetComponentInParent<UnitMovement>().applyForce(hitData.knockUp * Vector3.up);
        }
    }
    public struct AttackSegment
    {
        public List<AttackStageState> states;
        public WindState windup;
        public WindState winddown;
        public ActionState action;
        public GameObject groundTargetInstance;


        public void enterSegment(UnitMovement mover)
        {
            HitInstanceData source = action.getSource();
            if (source.type == HitType.Ground)
            {
                GameObject body = mover.getSpawnBody();
                Size s = body.GetComponentInChildren<Size>();
                groundTargetInstance = SpawnGroundTarget(body.transform, s.scaledRadius, mover.transform.position + mover.input.lookOffset, source.length);
                groundTargetInstance.GetComponent<GroundTarget>().height = s.indicatorHeight;
                windup.setGroundTarget(groundTargetInstance, new FloorNormal.GroundSearchParams
                {
                    radius = s.scaledRadius,
                    distance = s.scaledHalfHeight,
                });
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
            AttackStageState state = states[0];
            states.RemoveAt(0);
            return state;
        }
    }


    static GameObject SpawnGroundTarget(Transform body, float radius, Vector3 target, float length)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().GroundTargetPre;
        Vector3 bodyFocus = body.position + body.forward * radius;
        Vector3 diff = target - bodyFocus;
        Vector3 planarDiff = diff;
        planarDiff.y = 0;

        //float angleOff = Vector3.SignedAngle(body.forward, planarDiff, Vector3.up);
        //Vector3 forwardLine = Quaternion.AngleAxis(angleOff, Vector3.up) * diff;
        //Vector3 offset = forwardLine.normalized * Mathf.Min(length, Vector3.Dot(diff, forwardLine));
        Vector3 offset = body.forward * Mathf.Max(Mathf.Min(length, Vector3.Dot(planarDiff, body.forward)), 0);


        GameObject instance = GameObject.Instantiate(prefab, bodyFocus + offset, Quaternion.identity);
        NetworkServer.Spawn(instance);
        return instance;
    }



    public static void SpawnProjectile(Transform body, float radius, float halfHeight, UnitMovement mover, HitInstanceData hitData)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().ProjectilePre;
        Vector3 bodyFocus = body.position + body.forward * radius;
        GameObject instance = GameObject.Instantiate(prefab, bodyFocus, body.rotation);
        Projectile p = instance.GetComponent<Projectile>();
        float hitRadius = hitData.width / 2;
        float terrainRadius = Mathf.Min(hitRadius, halfHeight * 0.5f);
        p.init(terrainRadius, hitRadius, halfHeight, mover, hitData);
        NetworkServer.Spawn(instance);
    }
    public static List<GameObject> LineAttack(Transform body, float radius, float halfHeight, float length, float width)
    {
        float playerHeightOversize = halfHeight * 2 * 1.5f;
        List<GameObject> hits = new List<GameObject>();
        List<GameObject> tempHits = new List<GameObject>();
        Vector3 bodyFocus = body.position + body.forward * radius;
        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        Vector3 boxCenter = bodyFocus + maxDistance * 0.5f * body.forward;
        float boxHeight = Mathf.Max(playerHeightOversize, maxDistance);
        Vector3 boxHalfs = new Vector3(width / 2, boxHeight / 2, maxDistance / 2);

        Quaternion q = Quaternion.LookRotation(body.forward);
        RaycastHit[] boxHits = Physics.BoxCastAll(boxCenter, boxHalfs, body.forward, q, 0.0f, LayerMask.GetMask("Players"));
        //RaycastHit[] sphereHits = Physics.SphereCastAll(bodyFocus, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players"));
        float capsuleHeightFactor = Mathf.Max(boxHeight / 2 - maxDistance, 0);
        Vector3 capsuleHeightDiff = body.up * capsuleHeightFactor;
        Vector3 capsuleStart = bodyFocus + capsuleHeightDiff;
        Vector3 capsuleEnd = bodyFocus - capsuleHeightDiff;
        RaycastHit[] capsuleHits = Physics.CapsuleCastAll(capsuleStart, capsuleEnd, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players"));

        //Debug.DrawLine(bodyFocus, bodyFocus + body.forward * maxDistance, Color.blue, 3.0f); ;
        //Debug.DrawLine(bodyFocus, bodyFocus + (body.forward+body.up).normalized * maxDistance, Color.blue, 3.0f);
        //DrawBox(boxCenter, q,boxHalfs*2, Color.blue);
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
}
