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

public class SaveData : NetworkBehaviour
{

    Auth auth;
    Inventory inv;
    PlayerGhost player;
    PlayerPity pity;
    GlobalSaveData globalSave;
    // Start is called before the first frame update

    WorldProgress worldProgress;
    UIQuestDisplay questDisplay;

    public enum DataSource{
        Online,
        Offline
    }
    public static DataSource dataSource = DataSource.Online;

    void Start()
    {
        globalSave = FindObjectOfType<GlobalSaveData>();
        questDisplay = FindObjectOfType<UIQuestDisplay>(true);
        auth = GetComponent<Auth>();
        inv = GetComponent<Inventory>();
        pity = GetComponent<PlayerPity>();
        player = GetComponent<PlayerGhost>();


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
        if(dataSource == DataSource.Online)
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
        Task<DataSnapshot> equipped = tasks.equippedItems;
        Task<DataSnapshot> power = tasks.power;
        Task<DataSnapshot> pityData = tasks.pity;
        Task<DataSnapshot> quests = tasks.quests;

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
            || (!equipped.IsFaulted && !equipped.IsCompleted && !equipped.IsCanceled))
        {
            yield return null;
        }

        if (storage.IsFaulted || equipped.IsFaulted)
        {
            Debug.LogError("Error loading inv");
        }
        else if (storage.IsCompleted && equipped.IsCompleted)
        {
            DataSnapshot snapshotStorage = storage.Result;
            DataSnapshot snapshotEquipped = equipped.Result;
            if (snapshotEquipped.Exists)
            {
                AttackBlock[] storageData = new AttackBlock[0];
                if (snapshotStorage.Exists)
                {
                    storageData = globalSave.itemsFromSnapshot(snapshotStorage);
                }
                AttackBlock[] equippedData = globalSave.itemsFromSnapshot(snapshotEquipped);
                inv.reloadItems(storageData, equippedData);
            }
            else
            {
                inv.genMinItems();

            }
        }




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
    }
    private void OnDestroy()
    {
        if (isServer)
        {
            globalSave.savePlayerData(auth.user, new PlayerSaveData
            {
                power = player.power,
                pitySave = pity.save(),
                worldProgress = worldProgress,
            });
            saveItems();
        }

    }

    //server
    public void saveItems()
    {
        globalSave.savePlayerItems(auth.user, inv.exportEquipped(), inv.exportStorage());
    }
}
