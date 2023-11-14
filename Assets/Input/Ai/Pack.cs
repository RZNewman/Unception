using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static EventManager;

public class Pack : NetworkBehaviour
{
    List<GameObject> pack = new List<GameObject>();
    public GameObject spawnWarning;


    [HideInInspector]
    public float scale;

    SoundManager sound;

    Encounter encounter;

    //Server
    bool aggroed = false;
    bool enabledUnits = false;
    private void Start()
    {
        sound = FindObjectOfType<SoundManager>();
        disableUnits();
    }

    public float rewardPercent
    {
        get
        {
            return pack.Sum(u => u.GetComponent<Reward>().rewardPercent);
        }
    }

    public void setEncounter(Encounter e)
    {
        encounter = e;
        List<Color> ind = new List<Color>() { Color.red };
        foreach(GameObject u in pack)
        {
            u.GetComponent<EventManager>().AggroEvent += (GameObject col) => encounter.trySetCombat(col);
            u.GetComponent<UnitChampInd>().setColors(ind);
        }
    }

    List<GameObject> players = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.GetComponentInParent<TeamOwnership>().getTeam() == TeamOwnership.PLAYER_TEAM)
            {
                players.Add(other.gameObject);
                if (!enabledUnits)
                {
                    enabledUnits = true;
                    enableUnits();
                }

            }

        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (isServer && !aggroed)
        {
            players.Remove(other.gameObject);
            if (players.Count == 0)
            {
                disableUnits();
                enabledUnits = false;
            }
        }

    }



    public void addToPack(GameObject u)
    {
        pack.Add(u);
    }
    public void packDeath(GameObject u)
    {
        pack.Remove(u);
    }
    public int unitCount
    {
        get { return pack.Count; }
    }

    public bool packAlive()
    {
        return pack.Count > 0;
    }


    [Server]
    public void disableItemReward()
    {
        foreach (GameObject u in pack)
        {
            u.GetComponent<Reward>().givesItems = false;
        }

    }
    [Server]
    public void disableUnits()
    {
        foreach (GameObject u in pack)
        {
            u.GetComponent<EventManager>().suscribeDeath((bool _) => pack.Remove(u));
            u.SetActive(false);
        }
        RpcDisablePack(pack);
    }

    [ClientRpc]
    void RpcDisablePack(List<GameObject> packSync)
    {
        if (isClientOnly)
        {
            foreach (GameObject u in packSync)
            {
                u.SetActive(false);
            }
        }

    }

    [Server]
    public void enableUnits()
    {
        foreach (GameObject u in pack)
        {
            u.SetActive(true);
        }
        RpcEnablePack(pack);
    }

    [ClientRpc]
    void RpcEnablePack(List<GameObject> packSync)
    {
        if (isClientOnly)
        {
            foreach (GameObject u in packSync)
            {
                u.SetActive(true);
            }
        }

    }
    [Server]
    public void enableUnitsChain(Vector3 location)
    {
        pack = pack.OrderBy(p => (p.transform.position - location).magnitude).Reverse().ToList();
        StartCoroutine(unitChainCoroutine());
    }

    //server
    IEnumerator unitChainCoroutine()
    {
        foreach (GameObject u in pack)
        {
            StartCoroutine(unitSpawnCoroutine(u));
            yield return new WaitForSeconds(0.3f);
        }

    }
    //server
    IEnumerator unitSpawnCoroutine(GameObject u)
    {
        GameObject o = Instantiate(spawnWarning, u.transform.position, Quaternion.identity);
        o.transform.localScale = scale * Vector3.one;
        NetworkServer.Spawn(o);
        yield return new WaitForSeconds(1f);
        Destroy(o);
        sound.sendSound(SoundManager.SoundClip.EncounterSpawn, u.transform.position);
        u.SetActive(true);
    }


    public void packAggro(GameObject target)
    {
        aggroed = true;
        foreach (GameObject a in pack)
        {
            a.GetComponent<ControlManager>().spawnControl(); //spawn early so we can access aggro in encoutners
            a.GetComponentInChildren<AggroHandler>().addAggro(target);
        }
    }

    public void reposition(List<Vector3> positions)
    {
        positions.Shuffle();
        for (int i = 0; i < pack.Count; i++)
        {
            pack[i].transform.position = positions[i];
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject a in pack)
        {
            Destroy(a);
        }
    }
}
