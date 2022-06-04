using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    MapGenerator gen;

    public void setGen(MapGenerator m)
    {
        gen = m;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject them = other.gameObject;
        uint theirTeam = them.GetComponentInParent<TeamOwnership>().getTeam();
        if (theirTeam == 1u)
        {
            StartCoroutine(makeNextLevel());
        }
    }

    IEnumerator makeNextLevel()
    {
        gen.buildNewLevel(transform.position + Vector3.down * 20);
        yield return new WaitForSecondsRealtime(2.5f);
        //TODO teleport non-local players
        gen.cleanupLevel();
    }
}
