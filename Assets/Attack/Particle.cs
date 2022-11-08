using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Particle : MonoBehaviour
{
    public GameObject visualScale;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(cleanup());
    }

    public void setVisuals(GameObject prefab)
    {
        Instantiate(prefab, visualScale.transform);
    }

    IEnumerator cleanup()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
