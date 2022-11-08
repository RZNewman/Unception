using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GlobalPrefab : MonoBehaviour
{
    public GameObject AbilityRootPre;
    public GameObject ProjectilePre;
    public GameObject GroundTargetPre;
    public GameObject ItemDropPre;

    public GameObject LineIndPre;
    public GameObject GroundIndPre;
    public GameObject ProjIndPre;
    public GameObject DashIndPre;

    public VisualEffectAsset[] projectileAssets;
    public VisualEffectAsset[] lineAssets;
    public VisualEffectAsset[] groundAssets;
}
