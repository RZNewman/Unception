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
        Vector3Int start = new Vector3Int(6, 6 + Mathf.RoundToInt(Random.value * 20), 6);
        List<Vector3Int> path = new List<Vector3Int>();
        path.Add(start);

        int points = 2;
        for (int i = 0; i < points; i++)
        {
            path.Add(new Vector3Int(
                10 + Mathf.RoundToInt(Random.value * 40),
                6 + Mathf.RoundToInt(Random.value * 20),
                10 + Mathf.RoundToInt(Random.value * 40)
                ));
        }
        return path;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
