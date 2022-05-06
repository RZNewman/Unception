using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColorTarget : MonoBehaviour
{
    public List<GameObject> targets;


    private void Start()
    {
        Material material = GetComponentInParent<UnitPropsHolder>().props.material;
        foreach (GameObject target in targets)
        {
            target.GetComponent<MeshRenderer>().material = material;
        }
    }
}
