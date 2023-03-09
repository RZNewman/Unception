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

public class SaveData : NetworkBehaviour
{
    
    Auth auth;
    Inventory inv;
    PlayerGhost player;
    GlobalSaveData globalSave;
    // Start is called before the first frame update

    void Start()
    {
        globalSave = FindObjectOfType<GlobalSaveData>();
        auth = GetComponent<Auth>();
        inv = GetComponent<Inventory>();
        player = GetComponent<PlayerGhost>();
        

    }







    [Server]
    public void loadData()
    {
        StartCoroutine(loadDataRoutine());
        //use below for offline dev
        //loadDataOffline();
    }



    IEnumerator loadDataRoutine()
    {
        
        PlayerLoadTasks tasks = globalSave.getLoadTasks(auth.user);
        Task<DataSnapshot> items = tasks.items;
        Task<DataSnapshot> power = tasks.power;
        Task<DataSnapshot> pity = tasks.pity;

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

        while (!pity.IsFaulted && !pity.IsCompleted && !pity.IsCanceled)
        {
            yield return null;
        }


        if (pity.IsFaulted)
        {
            Debug.LogError("Error loading pity");
        }
        else if (pity.IsCompleted)
        {
            DataSnapshot snapshot = pity.Result;
            if (snapshot.Exists)
            {
                inv.loadPity(JsonConvert.DeserializeObject<Dictionary<string, float>>(snapshot.GetRawJsonValue()));
            }
            else
            {
                inv.createBasePity();
            }

        }

        while (!items.IsFaulted && !items.IsCompleted && !items.IsCanceled)
        {
            yield return null;
        }

        if (items.IsFaulted)
        {
            Debug.LogError("Error loading inv");
        }
        else if (items.IsCompleted)
        {
            DataSnapshot snapshot = items.Result;
            if (snapshot.Exists)
            {
                AttackBlock[] itemData = globalSave.itemsFromSnapshot(snapshot);
                inv.reloadItems(itemData);
            }
            else
            {
                inv.reloadItems(new AttackBlock[0]);

            }
        }




    }

    void loadDataOffline()
    {
        if (FindObjectOfType<GlobalPlayer>().serverPlayer == player)
        {
            FindObjectOfType<Atlas>(true).makeMaps();
        }
        inv.createBasePity();
        inv.genRandomItems();
    }
    private void OnDestroy()
    {
        if (isServer)
        {
            globalSave.savePlayerData(auth.user, new PlayerSaveData
            {
                power = player.power,
                pity = inv.savePity(),
            });
            saveItems();
        }

    }

    //server
    public void saveItems()
    {
        inv.clearDelete();
        globalSave.savePlayerItems(auth.user, inv.exportItems());
    }
}
