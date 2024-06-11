using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class PartScaler : MonoBehaviour
{
    public Vector3 min =Vector3.one;
    public Vector3 max = Vector3.one;

    public Vector3 random()
    {
        return RandomScale(min, max);
    }

    public void scale(Vector3 s)
    {
        transform.localScale = s;
    }
}
