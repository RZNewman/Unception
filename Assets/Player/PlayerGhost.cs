using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static GlobalSaveData;
using static Power;

public class PlayerGhost : NetworkBehaviour, TextValue
{
    public GameObject unitPre;

    public int attacksToGenerate = 4;

    [SyncVar]
    GameObject currentSelf;

    [SyncVar]
    float playerPower = Power.playerStartingPower;

    [SyncVar]
    int extraLives = 1;

    AudioListener listener;
    MusicBox music;
    SaveData save;
    Inventory inv;
    PlayerPity pity;
    void Start()
    {
        inv = GetComponent<Inventory>();
        pity = GetComponent<PlayerPity>();
        save = GetComponent<SaveData>();
        music = FindObjectOfType<MusicBox>(true);
        listener = music.GetComponent<AudioListener>();
        if (isLocalPlayer)
        {
            FindObjectOfType<GlobalPlayer>().setLocalPlayer(this);
            FindObjectOfType<MenuHandler>().clientMenu();
            FindObjectOfType<PlayerUiReference>(true).setTarget(this);
            foreach (UiInvLimit limit in FindObjectsOfType<UiInvLimit>(true))
            {
                limit.set(inv);
            }


        }
        else
        {
            listener.enabled = false;
        }
        if (isServer)
        {
            FindObjectOfType<GlobalPlayer>().setServerPlayer(this);
        }
        if (isClient)
        {
            inv.subscribeInventory(FindObjectOfType<Atlas>().inventoryChange);

        }
    }
    public float power
    {
        get
        {
            return playerPower;
        }
    }
    public WorldProgress progress
    {
        get
        {
            return save.progress;
        }
    }
    public PlayerPity pityTimers
    {
        get
        {
            return pity;
        }
    }

    [Server]
    public void refreshLives()
    {
        extraLives = 1;
    }

    public void embark(int mapIndex)
    {
        CmdEmbark(mapIndex);
    }

    [Command]
    void CmdEmbark(int mapIndex)
    {
        //TODO every player
        refreshLives();
        inv.clearDrops();
        save.saveItems();
        StartCoroutine(embarkRoutine(mapIndex));
    }

    [Server]
    IEnumerator embarkRoutine(int mapIndex)
    {

        Atlas atlas = FindObjectOfType<Atlas>();
        yield return atlas.embarkServer(mapIndex);

        if (!atlas.embarked)
        {
            yield break;
        }
        buildUnit();

        TargetGameplayMenu(connectionToClient);
    }

    [Server]
    void buildUnit()
    {

        GameObject u = Instantiate(unitPre);
        Power p = u.GetComponent<Power>();
        p.setPower(playerPower, Atlas.softcap);
        p.subscribePower(syncPower);
        u.GetComponent<Reward>().setInventory(inv);
        u.GetComponent<EventManager>().suscribeDeath(onUnitDeath);
        NetworkServer.Spawn(u, connectionToClient);
        u.GetComponent<AbiltyManager>().addAbility(inv.equippedAbilities);
        currentSelf = u;
        RpcSetAudio(false);
    }

    [TargetRpc]
    void TargetGameplayMenu(NetworkConnection conn)
    {
        music.Game();
        FindObjectOfType<MenuHandler>().gameplayMenu();
    }
    [TargetRpc]
    public void TargetMainMenu(NetworkConnection conn)
    {
        music.Menu();
        FindObjectOfType<MenuHandler>().loadoutMenu();
    }


    [ClientRpc]
    public void RpcSetAudio(bool audio)
    {
        setAudio(audio);
    }
    void setAudio(bool audio)
    {
        if (isLocalPlayer)
        {
            listener.enabled = audio;
            if (audio)
            {
                listener.gameObject.transform.position = transform.position;
            }
        }
    }


    //server
    void onUnitDeath(bool natural)
    {
        RpcSetAudio(true);
        currentSelf = null;
        Atlas atlas = FindObjectOfType<Atlas>();
        if (atlas && atlas.embarked)
        {
            StartCoroutine(onDeathRoutine(atlas));

        }
    }
    public bool extraLife
    {
        get
        {
            return extraLives > 0;
        }
    }

    IEnumerator onDeathRoutine(Atlas atlas)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (extraLives > 0)
        {
            extraLives--;
            buildUnit();
        }
        else
        {
            atlas.disembark(false);
        }

    }

    public GameObject unit
    {
        get { return currentSelf; }
    }

    [Server]
    void syncPower(Power p)
    {
        playerPower = p.power;
    }

    [Server]
    public void setPower(float p)
    {
        playerPower = p;
    }

    [ClientRpc]
    public void RpcSetCompassDirection(Vector3 dir)
    {
        FindObjectOfType<Compass>(true).setDirection(dir);
    }

    public void pause(bool paused)
    {
        if (currentSelf)
        {
            currentSelf.GetComponentInChildren<LocalCamera>().pause(paused);
        }
    }

    public TextValue.TextData getText()
    {
        return new TextValue.TextData
        {
            color = Color.white,
            text = displayExaggertatedPower(playerPower),

        };
    }
}
