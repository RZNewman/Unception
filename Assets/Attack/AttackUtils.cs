using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;

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
            switch (hitData.knockBackType)
            {
                case KnockBackType.inDirection:
                    other.GetComponentInParent<UnitMovement>().applyForce(hitData.knockback * knockbackData.direction);
                    break;
                case KnockBackType.fromCenter:
                    Vector3 dir = other.transform.position - knockbackData.center;
                    dir.y = 0;
                    dir.Normalize();
                    other.GetComponentInParent<UnitMovement>().applyForce(hitData.knockback * dir);
                    break;
            }
            other.GetComponentInParent<UnitMovement>().applyForce(hitData.knockUp * Vector3.up);
        }
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
