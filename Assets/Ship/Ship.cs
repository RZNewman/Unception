using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public GameObject shipWaterPosition;
    public GameObject shipWaterArrow;
    // Start is called before the first frame update
    void Start()
    {
        shipWaterArrow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
