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

public class SaveData : NetworkBehaviour
{
    DatabaseReference db;
    JsonSerializerSettings settings;
    Auth auth;
    Inventory inv;
    PlayerGhost player;
    // Start is called before the first frame update

    void Start()
    {
        auth = GetComponent<Auth>();
        inv = GetComponent<Inventory>();
        player = GetComponent<PlayerGhost>();
        db = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        };

    }

    string santitizeJson(string json)
    {
        return json.Replace('$', '@');
    }
    string unsantitizeJson(string json)
    {
        return json.Replace('@', '$');
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
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        Task<DataSnapshot> items = db.Child("Characters").Child(auth.user).Child("items").GetValueAsync();
        Task<DataSnapshot> power = db.Child("Characters").Child(auth.user).Child("power").GetValueAsync();
        Task<DataSnapshot> pity = db.Child("Characters").Child(auth.user).Child("pityQuality").GetValueAsync();

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
                AttackBlock[] itemData = JsonConvert.DeserializeObject<AttackBlock[]>(unsantitizeJson(snapshot.GetRawJsonValue()), settings);
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
            db.Child("Characters").Child(auth.user).Child("power").SetValueAsync(player.power);
            db.Child("Characters").Child(auth.user).Child("pityQuality").SetRawJsonValueAsync(JsonConvert.SerializeObject(inv.savePity()));
            saveItems();
        }

    }

    //server
    public void saveItems()
    {
        inv.clearDelete();
        string json = santitizeJson(JsonConvert.SerializeObject(inv.exportItems(), Formatting.None, settings));
        db.Child("Characters").Child(auth.user).Child("items").SetRawJsonValueAsync(json);
    }
}
