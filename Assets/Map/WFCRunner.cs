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
        StartCoroutine(generation.collapseCells(1, 1, 1, genSize.x, genSize.y, genSize.z));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
