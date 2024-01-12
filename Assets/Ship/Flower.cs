using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public GameObject seedPre;
    public GameObject cameraPlant;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void shoot(Action<Vector3> callback)
    {
        StartCoroutine(shootRoutine(callback));
    }

    IEnumerator shootRoutine(Action<Vector3> callback)
    {
        cameraPlant.SetActive(true);
        yield return new WaitForSeconds(2f);
        spawnSeed(callback);
    }
    void spawnSeed(Action<Vector3> callback)
    {
        GameObject o = Instantiate(seedPre, transform.position + transform.up * 20, Quaternion.identity);
        o.GetComponent<Arc>().init(transform.up, 45f, callback);
        NetworkServer.Spawn(o);
    }
}
