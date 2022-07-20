using System.Linq;
using UnityEngine;

public class UnitColorTarget : MonoBehaviour
{
    public GameObject[] targets;


    public void colorTargets(Material[] materials)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i];
            target.GetComponent<Renderer>().material = materials[i];
        }



    }
    public Material[] getSource()
    {
        return targets.Select((t) => t.GetComponent<Renderer>().sharedMaterial).ToArray();
    }
}
