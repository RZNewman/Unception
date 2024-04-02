using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UnitSound;

public class Particle : NetworkBehaviour
{
    public GameObject visualScaleLine;
    public GameObject visualScaleCircle;
    public GameObject visualScaleProjectile;

    public enum VisualType
    {
        Line,
        Circle,
        Projectile,
        FullCircle,
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
        GameObject prefab = visualType switch
        {
            VisualType.Line => gp.lineAssetsPre[visIndex],
            VisualType.Circle => gp.groundAssetsPre[visIndex],
            VisualType.Projectile => gp.projectileAssetsPre[visIndex],
            VisualType.FullCircle => gp.circleAssetsPre[visIndex],
            _ => throw new NotImplementedException()
        };
        GameObject parent = visualType switch
        {
            VisualType.Line => visualScaleLine,
            VisualType.Projectile =>visualScaleProjectile,
            _ => visualScaleCircle
        };
        setAudioDistances(Instantiate(prefab, parent.transform), dists);
        if (!transform.parent)
        {
            StartCoroutine(cleanup());
        }
        
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
