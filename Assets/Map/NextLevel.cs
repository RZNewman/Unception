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
            float playerPower = them.GetComponentInParent<Power>().power;
            StartCoroutine(makeNextLevel(playerPower));
        }
    }

    IEnumerator makeNextLevel(float power)
    {
        gen.endOfLevel(transform.position + Vector3.down * 30, power);
        yield return new WaitForSecondsRealtime(2.5f);
        //TODO teleport non-local players
        gen.cleanupLevel();
    }
}
