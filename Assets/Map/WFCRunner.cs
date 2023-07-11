using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCRunner : MonoBehaviour
{
    WFCGeneration generation;
    // Start is called before the first frame update
    void Start()
    {
        generation = GetComponent<WFCGeneration>();
        generation.init();
        StartCoroutine(generation.collapseCells(1, 1, 1, 90, 25, 90));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
