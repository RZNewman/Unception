using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Newtonsoft.Json;
using Firebase.Extensions;
using System.Linq;
using static UnityEditor.Progress;
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
        settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,

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



    public void saveItem(AttackBlock block)
    {

        string json = santitizeJson(JsonConvert.SerializeObject(block, Formatting.None, settings));
        db.Child("Characters").Child(auth.user).Child("items").Push().SetRawJsonValueAsync(json);
    }

    [Server]
    public void loadData()
    {
        StartCoroutine(loadDataRoutine());
    }

    
    IEnumerator loadDataRoutine()
    {
        Task<DataSnapshot> items = db.Child("Characters").Child(auth.user).Child("items").GetValueAsync();
        Task<DataSnapshot> power = db.Child("Characters").Child(auth.user).Child("power").GetValueAsync();

        while(!items.IsFaulted && !items.IsCompleted && !items.IsCanceled)
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
                AttackBlock[] itemData = JsonConvert.DeserializeObject<Dictionary<string, AttackBlock>>(unsantitizeJson(snapshot.GetRawJsonValue()), settings).Values.ToArray();
                inv.reloadItems(itemData);
            }
            else
            {
                inv.reloadItems(new AttackBlock[0]);

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
                player.setPower(Convert.ToSingle( snapshot.Value));
            }

            if (FindObjectOfType<GlobalPlayer>().serverPlayer == player)
            {
                FindObjectOfType<Atlas>(true).makeMaps();
            }

        }

        
    }

    private void OnDestroy()
    {
        if (isServer)
        {
            db.Child("Characters").Child(auth.user).Child("power").SetValueAsync(player.power);
        }

    }
}
