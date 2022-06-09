using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;
public static class InstantHit
{
    public static List<GameObject> LineAttack(Transform body,float raduis, float halfHeight, float length, float width)
    {
        float totalHeight = halfHeight * 2 * 1.5f;
        List<GameObject> hits = new List<GameObject>();
        List<GameObject> tempHits = new List<GameObject>();
        Vector3 bodyFocus = body.position + body.forward * raduis;
        Vector2 attackVec = new Vector2(length, width/2);
        float maxDistance = attackVec.magnitude;
        Vector3 boxCenter = bodyFocus + maxDistance * 0.5f * body.forward;
        Vector3 boxHalfs = new Vector3(width / 2, totalHeight/2, maxDistance / 2);

        Quaternion q = Quaternion.LookRotation(body.forward);
        RaycastHit[] boxHits = Physics.BoxCastAll(boxCenter, boxHalfs, body.forward, q, 0.0f, LayerMask.GetMask("Players"));
        //RaycastHit[] sphereHits = Physics.SphereCastAll(bodyFocus, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players"));
        Vector3 capsuleHeightDiff = body.up * totalHeight / 2; //height multiplier
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
