using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

public class GlobalSaveData : MonoBehaviour
{
    DatabaseReference db;
    public static readonly JsonSerializerSettings JSONsettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
    };
    // Start is called before the first frame update
    void Start()
    {
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        db = FirebaseDatabase.DefaultInstance.RootReference;
        
    }

    static string santitizeJson(string json)
    {
        return json.Replace('$', '@');
    }
    static string unsantitizeJson(string json)
    {
        return json.Replace('@', '$');
    }
    public struct PlayerLoadTasks
    {
        public Task<DataSnapshot> items;
        public Task<DataSnapshot> power;
        public Task<DataSnapshot> pity;
    }
    public PlayerLoadTasks getLoadTasks(string playerName)
    {
        return new PlayerLoadTasks
        {
            items = db.Child("Characters").Child(playerName).Child("items").GetValueAsync(),
            power = db.Child("Characters").Child(playerName).Child("power").GetValueAsync(),
            pity = db.Child("Characters").Child(playerName).Child("pityQuality").GetValueAsync(),
        };
    }

    public AttackBlock[] itemsFromSnapshot(DataSnapshot snapshot)
    {
        return JsonConvert.DeserializeObject<AttackBlock[]>(unsantitizeJson(snapshot.GetRawJsonValue()), JSONsettings);
    }
    public delegate void AssignItems(List<AttackBlock> blocks);

    public IEnumerator championItems(AssignItems assign)
    {

        Task<DataSnapshot> abilities = db.Child("Champions").GetValueAsync();
        while (!abilities.IsFaulted && !abilities.IsCompleted && !abilities.IsCanceled)
        {
            yield return null;
        }

        if (abilities.IsFaulted)
        {
            Debug.LogError("Error loading champion abilities");
        }
        else if (abilities.IsCompleted)
        {
            DataSnapshot snapshot = abilities.Result;

            Dictionary<string, AttackBlock> abilityData = JsonConvert.DeserializeObject<Dictionary<string, AttackBlock>>(unsantitizeJson(snapshot.GetRawJsonValue()), JSONsettings);
            assign(abilityData.Values.ToList());

        }

    }

    public struct PlayerSaveData
    {
        public float power;
        public Dictionary<string, float> pity;
    }

    public void savePlayerData(string playerName, PlayerSaveData data)
    {
        db.Child("Characters").Child(playerName).Child("power").SetValueAsync(data.power);
        db.Child("Characters").Child(playerName).Child("pityQuality").SetRawJsonValueAsync(JsonConvert.SerializeObject(data.pity));
    }

    public void savePlayerItems(string playerName, AttackBlock[] items)
    {
        string json = santitizeJson(JsonConvert.SerializeObject(items, Formatting.None, JSONsettings));
        db.Child("Characters").Child(playerName).Child("items").SetRawJsonValueAsync(json);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
