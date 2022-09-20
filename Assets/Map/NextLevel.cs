using Mirror;
using System.Collections;
using UnityEngine;

public class NextLevel : NetworkBehaviour
{
    MapGenerator gen;

    public void setGen(MapGenerator m)
    {
        gen = m;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
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
        gen.buildNewLevel(transform.position + Vector3.down * 20, power);
        yield return new WaitForSecondsRealtime(2.5f);
        //TODO teleport non-local players
        gen.cleanupLevel();
    }
}
