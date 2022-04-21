using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InstantHit
{
    public static List<GameObject> LineAttack(Transform body,float raduis,float length, float width)
    {
        List<GameObject> hits = new List<GameObject>();
        List<GameObject> tempHits = new List<GameObject>();
        Vector3 bodyFocus = body.position + body.forward * raduis;
        Vector2 attackVec = new Vector2(length, width);
        float maxDistance = attackVec.magnitude;
        Vector3 boxCenter = bodyFocus + maxDistance * 0.5f * body.forward;
        Vector3 boxHalfs = new Vector3(width / 2, maxDistance / 2, maxDistance / 2);

        RaycastHit[] boxHits = Physics.BoxCastAll(boxCenter, boxHalfs, body.forward, Quaternion.identity, 0.0f, LayerMask.GetMask("Players"));
        RaycastHit[] sphereHits = Physics.SphereCastAll(bodyFocus, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players"));

        Debug.DrawLine(bodyFocus, bodyFocus + body.forward * maxDistance,Color.red,5.0f);
        Debug.DrawLine(boxCenter, boxCenter + boxHalfs, Color.blue, 5.0f);

        foreach (RaycastHit hit in boxHits)
        {
            GameObject obj = hit.collider.gameObject;
            tempHits.Add(obj);
        }
        foreach (RaycastHit hit in sphereHits)
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
