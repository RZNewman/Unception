using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static GlobalSaveData;
using static Power;
using static Interaction;

public class PlayerGhost : NetworkBehaviour, TextValue
{
    public GameObject unitPre;

    [SyncVar]
    GameObject currentSelf;

    [SyncVar]
    float playerPower = Atlas.playerStartingPower;

    [SyncVar]
    int extraLives = 1;


    MusicBox music;
    SaveData save;
    Inventory inv;
    PlayerPity pity;
    Atlas atlas;
    void Start()
    {
        inv = GetComponent<Inventory>();
        pity = GetComponent<PlayerPity>();
        save = GetComponent<SaveData>();
        music = FindObjectOfType<MusicBox>(true);
        atlas = FindObjectOfType<Atlas>();

        if (isLocalPlayer)
        {
            FindObjectOfType<GlobalPlayer>().setLocalPlayer(this);
            FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Login);
            FindObjectOfType<PlayerUiReference>(true).setTarget(this);
            foreach (UiInvLimit limit in FindObjectsOfType<UiInvLimit>(true))
            {
                limit.set(inv);
            }


        }

        if (isServer)
        {
            FindObjectOfType<GlobalPlayer>().setServerPlayer(this);
            //TODO multiplayer fix
            FindObjectOfType<GroveWorld>().transform.parent.GetComponentInChildren<Interaction>().setInteraction(GroveInteract);
            FindObjectOfType<Atlas>().mapPodium.GetComponentInChildren<Interaction>().setInteraction(AtlasInteract);
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
        FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Gameplay);
        CmdEmbark(mapIndex);
    }

    [Command]
    void CmdEmbark(int mapIndex)
    {
        //TODO every player
        refreshLives();
        inv.clearDrops();
        save.saveItems();
        save.saveBlessings();
        StartCoroutine(embarkRoutine(mapIndex));
    }

    [Server]
    IEnumerator embarkRoutine(int mapIndex)
    {
        yield return atlas.embarkServer(mapIndex);
    }

    public void doneLoading()
    {
        TargetWarmup(connectionToClient);
        shootUnit();
    }

    void shootUnit()
    {
        FindObjectOfType<Flower>().shoot(buildUnit);
    }

    [TargetRpc]
    void TargetWarmup(NetworkConnection conn)
    {
        Shader.WarmupAllShaders();
    }


    [Server]
    void buildUnit(Vector3 spawn)
    {
        GameObject u = Instantiate(unitPre, spawn, Quaternion.identity);
        Power p = u.GetComponent<Power>();
        p.setPower(playerPower, Atlas.softcap);
        p.subscribePower(syncPower);
        u.GetComponent<Reward>().setInventory(inv);
        u.GetComponent<EventManager>().suscribeDeath(onUnitDeath);
        NetworkServer.Spawn(u, connectionToClient);
        u.GetComponent<AbilityManager>().addAbility(inv.equippedAbilities);
        u.GetComponent<TriggerManager>().addTrigger(inv.blessings);
        currentSelf = u;
    }

    [Server]
    void GroveInteract(Interactor i)
    {
        if(i.gameObject == currentSelf)
        {
            Destroy(i.gameObject);
            TargetMenu(connectionToClient, MenuHandler.Menu.Loadout);
        }
    }
    [Server]
    void AtlasInteract(Interactor i)
    {
        if (i.gameObject == currentSelf)
        {
            TargetMenu(connectionToClient, MenuHandler.Menu.Map);
        }
    }
    [Client]
    public void GroveLeave()
    {
        FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Gameplay);
        shootUnit();
    }


    [TargetRpc]
    void TargetMenu(NetworkConnection conn, MenuHandler.Menu m)
    {
        FindObjectOfType<MenuHandler>().switchMenu(m);
    }
    [TargetRpc]
    public void TargetMenuFinish(NetworkConnection conn, bool blessing)
    {
        music.Menu();
        MenuHandler.Menu target = blessing switch
        {
            true => MenuHandler.Menu.Blessing,
            _ => MenuHandler.Menu.Loadout,
        };
        FindObjectOfType<MenuHandler>().switchMenu(target);
    }
    [Client]

    public void stuck()
    {
        CmdStuck();
    }

    [Command]
    void CmdStuck()
    {
        if (currentSelf)
        {
            currentSelf.transform.position = atlas.playerSpawn;
        }
    }


    //server
    void onUnitDeath(bool natural)
    {   
        currentSelf = null;
        Atlas atlas = FindObjectOfType<Atlas>();
        if (natural)
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
        }
        else
        {
            atlas.disembark(false);
        }
        shootUnit();

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
