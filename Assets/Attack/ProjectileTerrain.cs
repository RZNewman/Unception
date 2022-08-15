using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTerrain : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GetComponentInParent<Projectile>().onTerrainCollide(other);
    }
}
