using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UnitSound;

public class Particle : MonoBehaviour
{
    public GameObject visualScaleLine;
    public GameObject visualScaleCircle;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(cleanup());
    }

    public void setVisualsLine(GameObject prefab, AudioDistances dists)
    {
        setAudioDistances(Instantiate(prefab, visualScaleLine.transform),dists);

    }
    public void setVisualsCircle(GameObject prefab, AudioDistances dists)
    {
        setAudioDistances(Instantiate(prefab, visualScaleCircle.transform),dists);
    }

    

    IEnumerator cleanup()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
