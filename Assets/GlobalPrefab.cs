using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GlobalPrefab : MonoBehaviour
{
    public GameObject AbilityRootPre;
    public GameObject ProjectilePre;
    public GameObject BuffPre;
    public GameObject GroundTargetPre;
    public GameObject ItemDropPre;
    public GameObject WetstonePre;

    public GameObject ParticlePre;
    public GameObject DamageNumberPre;

    public GameObject LineIndPre;
    public GameObject GroundIndPre;
    public GameObject ProjIndPre;
    public GameObject DashIndPre;

    public GameObject[] projectileAssetsPre;
    public GameObject[] lineAssetsPre;
    public GameObject[] groundAssetsPre;


    public static GlobalPrefab gPre = null;

    private void Start()
    {
        gPre = this;
    }
}
