using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static RewardManager;

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
    public GameObject ShapeIndPre;

    public Sprite Common;
    public Sprite Uncommon;
    public Sprite Rare;
    public Sprite Epic;
    public Sprite Legendary;



    public GameObject[] projectileAssetsPre;
    public GameObject[] lineAssetsPre;
    public GameObject[] groundAssetsPre;


    public static GlobalPrefab gPre = null;

    public Sprite bgFromQuality(Quality q)
    {
        Sprite bg;
        switch (q)
        {
            case Quality.Common:
                bg = Common;
                break;
            case Quality.Uncommon:
                bg = Uncommon;
                break;
            case Quality.Rare:
                bg = Rare;
                break;
            case Quality.Epic:
                bg = Epic;
                break;
            case Quality.Legendary:
                bg = Legendary;
                break;
            default:
                bg = Common;
                break;

        }
        return bg;
    }

    private void Start()
    {
        gPre = this;
    }
}
