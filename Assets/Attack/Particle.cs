using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UnitSound;

public class Particle : NetworkBehaviour
{
    public GameObject visualScaleLine;
    public GameObject visualScaleCircle;

    public enum VisualType
    {
        Line,
        Circle,
    }

    [SyncVar]
    VisualType visualType;

    [SyncVar]
    int visIndex;

    [SyncVar]
    AudioDistances dists;

    // Start is called before the first frame update
    void Start()
    {
        GlobalPrefab gp = GlobalPrefab.gPre;
        GameObject prefab = visualType == VisualType.Line ? gp.lineAssetsPre[visIndex] : gp.groundAssetsPre[visIndex];
        setAudioDistances(Instantiate(prefab, visualType == VisualType.Line ? visualScaleLine.transform : visualScaleCircle.transform), dists);
        StartCoroutine(cleanup());
    }

    public void setVisuals(VisualType type, int index, AudioDistances d)
    {
        visualType = type;
        visIndex = index;
        dists = d;
    }

    

    IEnumerator cleanup()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
