using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartScaler : MonoBehaviour
{
    public Vector3 min =Vector3.one;
    public Vector3 max = Vector3.one;

    public Vector3 random()
    {
        return new Vector3(
            Mathf.Lerp(min.x, max.x, Random.value),
            Mathf.Lerp(min.y, max.y, Random.value),
            Mathf.Lerp(min.z, max.z, Random.value)
            );
    }

    public void scale(Vector3 s)
    {
        transform.localScale = s;
    }
}
