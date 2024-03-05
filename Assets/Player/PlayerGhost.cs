using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static GlobalSaveData;
using static Power;
using static Interaction;
using static GenerateAttack;

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
            FindObjectOfType<Flower>().transform.parent.GetComponentInChildren<Interaction>().setInteraction(PlantFeedInteract);
            FindObjectOfType<Flower>().transform.parent.GetComponentInChildren<Interaction>().setCondition(PlantFeedCondition);
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

    public Scales scales
    {
        get
        {
            return currentSelf ? currentSelf.GetComponent<Power>().getScales() : new Scales
            {
                numeric =1,
                world =1,
                time =1,
            };
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
        GetComponent<PlayerInfo>().FireTutorialEvent(PlayerInfo.TutorialEvent.MapSelect);
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
        u.GetComponent<UnitPropsHolder>().owningPlayer = gameObject;
        Power p = u.GetComponent<Power>();
        p.setPower(playerPower, Atlas.softcap);
        p.setOverrideDefault();
        p.subscribePower(syncPower);
        u.GetComponent<Reward>().setInventory(inv);
        u.GetComponent<EventManager>().suscribeDeath(onUnitDeath);
        NetworkServer.Spawn(u, connectionToClient);
        u.GetComponent<AbilityManager>().addAbility(inv.equippedAbilities);
        u.GetComponent<TriggerManager>().addTrigger(inv.blessings);
        currentSelf = u;
    }

    [Server]
    void PlantFeedInteract(Interactor i)
    {
        if (i.gameObject == currentSelf)
        {
            GameObject water = i.gameObject.GetComponent<UnitPropsHolder>().waterCarried;
            water.GetComponent<Wetstone>().consume();
        }
    }

    bool PlantFeedCondition(Interactor i)
    {
        return i.gameObject.GetComponent<UnitPropsHolder>().waterCarried;
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


    [Server]
    public void transitionShip(bool toShip)
    {
        if (currentSelf)
        {
            UnitPropsHolder props = currentSelf.GetComponent<UnitPropsHolder>();
            Power p = currentSelf.GetComponent<Power>();
            if (toShip && props.launchedPlayer)
            {
                
                p.setOverrideDefault();
                currentSelf.transform.position = GameObject.FindWithTag("Spawn").transform.position;
                props.launchedPlayer = false;
                currentSelf.GetComponent<Combat>().clearFighting();
                
            }
            else if(!toShip)
            {
                p.setOverrideNull();
                currentSelf.transform.position = atlas.playerSpawn;
                props.launchedPlayer = true;
                GetComponent<PlayerInfo>().FireTutorialEvent(PlayerInfo.TutorialEvent.Launch);
            }
            currentSelf.GetComponent<UnitMovement>().stop(true);
            TargetToggleShip(connectionToClient, toShip);
        }
    }

    [TargetRpc]
    void TargetToggleShip(NetworkConnection conn, bool toShip)
    {
        if (toShip)
        {
            FindObjectOfType<MaterialScaling>().none();
            music.Menu();
        }
        else
        {
            FindObjectOfType<MaterialScaling>().game(FindObjectOfType<LocalCamera>().cameraMagnitude);
            music.Game();
        }
        
    }
    public void cleanup()
    {
        if (currentSelf)
        {
            Destroy(currentSelf);
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
            return currentSelf && extraLives > 0;
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
            //TODO lock player out instead of atlas
            atlas.disembarkFailure();
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
    public void RpcSetCompassTarget(Vector3 target)
    {
        FindObjectOfType<Compass>(true).setTarget(target);
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
