using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColorTarget : MonoBehaviour
{
    public List<GameObject> targets;


    private void Start()
    {
        SharedMaterials mats = FindObjectOfType<SharedMaterials>();
        mats.getVisuals(GetComponentInParent<UnitPropsHolder>().props.visualsId, colorTargets);
    }
    private void colorTargets(Material material)
    {

        foreach (GameObject target in targets)
        {
            target.GetComponent<MeshRenderer>().material = material;
        }
    }
}
