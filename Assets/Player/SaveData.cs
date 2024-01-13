using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Newtonsoft.Json;
using Firebase.Extensions;
using System.Linq;
using Mirror;
using System.Threading.Tasks;
using System;
using static GlobalSaveData;
using static Atlas;
using static Grove;
using static PlayerInfo;

public class SaveData : NetworkBehaviour
{

    Auth auth;
    Inventory inv;
    Grove grove;
    PlayerGhost player;
    PlayerPity pity;
    PlayerInfo playerInfo;
    GlobalSaveData globalSave;
    GlobalPlayer gp;
    Atlas atlas;
    // Start is called before the first frame update

    WorldProgress worldProgress;
    UIQuestDisplay questDisplay;

    public enum DataSource
    {
        Online,
        Offline
    }
    public static DataSource dataSource = DataSource.Online;

    void Start()
    {
        globalSave = FindObjectOfType<GlobalSaveData>();
        questDisplay = FindObjectOfType<UIQuestDisplay>(true);
        atlas = FindObjectOfType<Atlas>(true);
        gp = FindObjectOfType<GlobalPlayer>(true);
        auth = GetComponent<Auth>();
        inv = GetComponent<Inventory>();
        pity = GetComponent<PlayerPity>();
        player = GetComponent<PlayerGhost>();
        grove = GetComponent<Grove>();
        playerInfo = GetComponent<PlayerInfo>();
    }


    public WorldProgress progress
    {
        get
        {
            return worldProgress;
        }
    }

    public void saveQuestProgress(QuestIds ids)
    {
        if (worldProgress.locations == null)
        {
            worldProgress.locations = new Dictionary<string, Dictionary<string, QuestVerticalProgress>>();
        }
        if (!worldProgress.locations.ContainsKey(ids.locationId))
        {
            worldProgress.locations[ids.locationId] = new Dictionary<string, QuestVerticalProgress>();
        }

        //TODO restrict to next level only
        worldProgress.locations[ids.locationId][ids.verticalId] = new QuestVerticalProgress() { highestTier = ids.tier };
        questDisplay.displayWorld(worldProgress);
    }



    [Server]
    public void loadData()
    {
        if (dataSource == DataSource.Online)
        {
            StartCoroutine(loadDataRoutine());
        }
        else
        {
            loadDataOffline();
        }

    }



    IEnumerator loadDataRoutine()
    {

        PlayerLoadTasks tasks = globalSave.getLoadTasks(auth.user);
        Task<DataSnapshot> storage = tasks.storageItems;
        Task<DataSnapshot> placements = tasks.placements;
        Task<DataSnapshot> power = tasks.power;
        Task<DataSnapshot> pityData = tasks.pity;
        Task<DataSnapshot> quests = tasks.quests;
        Task<DataSnapshot> blessings = tasks.blessings;
        Task<DataSnapshot> notifications = tasks.notifications;

        while (!notifications.IsFaulted && !notifications.IsCompleted && !notifications.IsCanceled)
        {
            yield return null;
        }


        if (notifications.IsFaulted)
        {
            Debug.LogError("Error loading notifications");
        }
        else if (notifications.IsCompleted)
        {
            DataSnapshot snapshot = notifications.Result;
            if (snapshot.Exists)
            {
                NotificationsData nd = JsonConvert.DeserializeObject<NotificationsData>(snapshot.GetRawJsonValue());
                playerInfo.load(nd);
            }
            else
            {
                playerInfo.clear();
            }

        }

        while (!quests.IsFaulted && !quests.IsCompleted && !quests.IsCanceled)
        {
            yield return null;
        }


        if (quests.IsFaulted)
        {
            Debug.LogError("Error loading quests");
        }
        else if (quests.IsCompleted)
        {
            DataSnapshot snapshot = quests.Result;
            if (snapshot.Exists)
            {
                worldProgress = JsonConvert.DeserializeObject<WorldProgress>(snapshot.GetRawJsonValue());
                questDisplay.displayWorld(worldProgress);
            }
            else
            {
                questDisplay.clear();
            }

        }

        while (!power.IsFaulted && !power.IsCompleted && !power.IsCanceled)
        {
            yield return null;
        }


        if (power.IsFaulted)
        {
            Debug.LogError("Error loading power");
        }
        else if (power.IsCompleted)
        {
            DataSnapshot snapshot = power.Result;
            if (snapshot.Exists)
            {
                player.setPower(Convert.ToSingle(snapshot.Value));
            }
            else
            {
                player.setPower(Atlas.playerStartingPower);
            }

            if (FindObjectOfType<GlobalPlayer>().serverPlayer == player)
            {
                FindObjectOfType<Atlas>(true).makeMaps();
            }

        }

        while (!pityData.IsFaulted && !pityData.IsCompleted && !pityData.IsCanceled)
        {
            yield return null;
        }


        if (pityData.IsFaulted)
        {
            Debug.LogError("Error loading pity");
        }
        else if (pityData.IsCompleted)
        {
            DataSnapshot snapshot = pityData.Result;
            if (snapshot.Exists)
            {
                pity.load(JsonConvert.DeserializeObject<PitySaveData>(snapshot.GetRawJsonValue()));
            }
            else
            {
                pity.create();
            }

        }
        

        while ((!storage.IsFaulted && !storage.IsCompleted && !storage.IsCanceled)
            || (!placements.IsFaulted && !placements.IsCompleted && !placements.IsCanceled))
        {
            yield return null;
        }

        if (storage.IsFaulted || placements.IsFaulted)
        {
            Debug.LogError("Error loading inv");
        }
        else if (storage.IsCompleted && placements.IsCompleted)
        {
            DataSnapshot snapshotStorage = storage.Result;
            DataSnapshot snapshotPlaced = placements.Result;
            if (snapshotPlaced.Exists)
            {
                CastData[] storageData = new CastData[0];
                if (snapshotStorage.Exists)
                {
                    storageData = globalSave.itemsFromSnapshot(snapshotStorage);
                }
                Dictionary<string, GrovePlacement> placedData = JsonConvert.DeserializeObject<Dictionary<string, GrovePlacement>>(snapshotPlaced.GetRawJsonValue());
                inv.reloadItems(storageData, placedData);
            }
            else
            {
                inv.genMinItems();

            }
        }

        while (!blessings.IsFaulted && !blessings.IsCompleted && !blessings.IsCanceled)
        {
            yield return null;
        }
        if (blessings.IsFaulted)
        {
            Debug.LogError("Error loading blessings");
        }
        else if (blessings.IsCompleted)
        {
            DataSnapshot snapshotBlessings = blessings.Result;
            if (snapshotBlessings.Exists)
            {
                TriggerData[] bless = globalSave.blessingsFromSnapshot(snapshotBlessings);
                inv.reloadBlessings(bless);
            }
            else
            {
                inv.genMinBlessings();
            }
        }

        doneLoading();

    }

    void loadDataOffline()
    {
        if (FindObjectOfType<GlobalPlayer>().serverPlayer == player)
        {
            FindObjectOfType<Atlas>(true).makeMaps();
        }
        pity.create();
        inv.genMinItems();
        inv.genRandomItems();
        doneLoading();
    }

    [Server]
    void doneLoading()
    {  
        if(gp.serverPlayer == player)
        {
            atlas.setScaleServer(1, Power.scaleNumerical(player.power));
        }
        FindObjectOfType<UILoadoutMenu>(true).loadInvMode();
        TargetDoneLoading(connectionToClient);
        player.doneLoading();
    }


    [TargetRpc]
    void TargetDoneLoading(NetworkConnection conn)
    {
        FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Gameplay);
    }

    private void OnDestroy()
    {
        if (isServer && dataSource == DataSource.Online)
        {
            saveAll();
        }

    }

    public void saveAll()
    {
        globalSave.savePlayerData(auth.user, new PlayerSaveData
        {
            power = player.power,
            pitySave = pity.save(),
            worldProgress = worldProgress,
            notifSave = playerInfo.save(),
        });
        saveItems();
        saveBlessings();
    }

    //server
    public void saveItems()
    {
        globalSave.savePlayerItems(auth.user, grove.exportPlacements(), inv.exportStorage());
    }

    public void saveBlessings()
    {
        globalSave.savePlayerBlessings(auth.user, inv.exportBlessings());
    }
}
