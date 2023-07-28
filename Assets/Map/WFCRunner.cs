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

    }

    public void run()
    {
        StartCoroutine(generation.collapseCells(makePath()));
    }

    List<Vector3Int> makePath()
    {
        Vector3Int point = Vector3Int.zero;
        List<Vector3Int> path = new List<Vector3Int>();
        path.Add(point);

        Vector3 diff = Vector3.zero;

        int points = 5;
        for (int i = 0; i < points; i++)
        {
            Vector2 dir2d = Random.insideUnitCircle.normalized;
            Vector3 dir = new Vector3(dir2d.x, 0, dir2d.y);

            if (i != 0)
            {
                Vector3 flatDiff = new Vector3(diff.x, 0, diff.z);
                float angle = Vector3.Angle(dir, flatDiff);
                angle *= 0.75f;
                dir = Vector3.RotateTowards(dir, flatDiff, Mathf.PI * angle / 180, 0);
            }
            dir = Vector3.RotateTowards(dir, Random.value > 0.5f ? Vector3.up : Vector3.down, Mathf.PI * 0.20f, 0);
            dir.Normalize();

            diff = dir * (4 + Random.value * 6);

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
