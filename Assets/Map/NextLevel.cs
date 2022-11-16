using Mirror;
using System.Collections;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    MapGenerator gen;

    [Server]
    public void setGen(MapGenerator m)
    {
        gen = m;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gen)
        {
            return;
        }
        GameObject them = other.gameObject;
        uint theirTeam = them.GetComponentInParent<TeamOwnership>().getTeam();
        if (theirTeam == 1u)
        {
            gen.endFloor(transform.position);
        }
    }
}
