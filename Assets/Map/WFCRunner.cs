using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCRunner : MonoBehaviour
{
    public Vector3Int genSize = new Vector3Int(20, 10, 20);
    WFCGeneration generation;
    // Start is called before the first frame update
    void Start()
    {
        generation = GetComponent<WFCGeneration>();
        generation.init();
        StartCoroutine(generation.collapseCells(makePath()));
    }

    List<Vector3Int> makePath()
    {
        Vector3Int point = Vector3Int.zero;
        List<Vector3Int> path = new List<Vector3Int>();
        path.Add(point);

        Vector3 diff = Vector3.zero;

        int points = 4;
        for (int i = 0; i < points; i++)
        {
            if (diff == Vector3.zero)
            {
                diff = Random.onUnitSphere * (7 + Random.value * 7);
            }
            else
            {
                diff = Vector3.RotateTowards(diff.normalized, Random.onUnitSphere, Mathf.PI * 1.5f, 0) * (7 + Random.value * 7);
            }
            point += diff.asInt();
            path.Add(point);
        }
        return path;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
