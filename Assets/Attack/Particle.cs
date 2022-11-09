using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Particle : MonoBehaviour
{
    public GameObject visualScaleLine;
    public GameObject visualScaleCircle;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(cleanup());
    }

    public void setVisualsLine(GameObject prefab)
    {
        Instantiate(prefab, visualScaleLine.transform);
    }
    public void setVisualsCircle(GameObject prefab)
    {
        Instantiate(prefab, visualScaleCircle.transform);
    }

    IEnumerator cleanup()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
