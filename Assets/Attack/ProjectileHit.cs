using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GetComponentInParent<Projectile>().onPlayerCollide(other);
    }
}
