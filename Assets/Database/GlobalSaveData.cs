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
        public Task<DataSnapshot> storageItems;
        public Task<DataSnapshot> equippedItems;
        public Task<DataSnapshot> blessings;
        public Task<DataSnapshot> power;
        public Task<DataSnapshot> pity;
        public Task<DataSnapshot> quests;
    }
    public PlayerLoadTasks getLoadTasks(string playerName)
    {
        return new PlayerLoadTasks
        {
            storageItems = db.Child("Characters").Child(playerName).Child("items").GetValueAsync(),
            equippedItems = db.Child("Characters").Child(playerName).Child("equipped").GetValueAsync(),
            power = db.Child("Characters").Child(playerName).Child("power").GetValueAsync(),
            pity = db.Child("Characters").Child(playerName).Child("pity").GetValueAsync(),
            quests = db.Child("Characters").Child(playerName).Child("quests").GetValueAsync(),
            blessings = db.Child("Characters").Child(playerName).Child("blessings").GetValueAsync(),
        };
    }

    public CastData[] itemsFromSnapshot(DataSnapshot snapshot)
    {
        return JsonConvert.DeserializeObject<CastData[]>(unsantitizeJson(snapshot.GetRawJsonValue()), JSONsettings);
    }
    public TriggerData[] blessingsFromSnapshot(DataSnapshot snapshot)
    {
        return JsonConvert.DeserializeObject<TriggerData[]>(unsantitizeJson(snapshot.GetRawJsonValue()), JSONsettings);
    }
    public delegate void AssignItems(List<CastData> blocks);

    //public IEnumerator championItems(AssignItems assign)
    //{
    //    while (db == null)
    //    {
    //        yield return null;
    //    }
    //    Task<DataSnapshot> abilities = db.Child("Champions").GetValueAsync();
    //    while (!abilities.IsFaulted && !abilities.IsCompleted && !abilities.IsCanceled)
    //    {
    //        yield return null;
    //    }

    //    if (abilities.IsFaulted)
    //    {
    //        Debug.LogError("Error loading champion abilities");
    //    }
    //    else if (abilities.IsCompleted)
    //    {
    //        DataSnapshot snapshot = abilities.Result;

    //        Dictionary<string, CastData> abilityData = JsonConvert.DeserializeObject<Dictionary<string, CastData>>(unsantitizeJson(snapshot.GetRawJsonValue()), JSONsettings);
    //        assign(abilityData.Values.ToList());

    //    }

    //}

    public struct PlayerSaveData
    {
        public float power;
        public PitySaveData pitySave;
        public WorldProgress worldProgress;
    }

    public struct PitySaveData
    {
        public Dictionary<string, float> quality;
        public Dictionary<string, float> breakables;
        public Dictionary<string, float> modCount;
        public Dictionary<string, float> modBonus;
    }

    public void savePlayerData(string playerName, PlayerSaveData data)
    {
        db.Child("Characters").Child(playerName).Child("power").SetValueAsync(data.power);
        db.Child("Characters").Child(playerName).Child("pity").SetRawJsonValueAsync(JsonConvert.SerializeObject(data.pitySave));
        db.Child("Characters").Child(playerName).Child("quests").SetRawJsonValueAsync(JsonConvert.SerializeObject(data.worldProgress));
    }

    public void savePlayerItems(string playerName, CastData[] equipped, CastData[] storage)
    {
        string json = santitizeJson(JsonConvert.SerializeObject(storage, Formatting.None, JSONsettings));
        db.Child("Characters").Child(playerName).Child("items").SetRawJsonValueAsync(json);
        json = santitizeJson(JsonConvert.SerializeObject(equipped, Formatting.None, JSONsettings));
        db.Child("Characters").Child(playerName).Child("equipped").SetRawJsonValueAsync(json);
    }

    public void savePlayerBlessings(string playerName, TriggerData[] blessings)
    {
        string json = santitizeJson(JsonConvert.SerializeObject(blessings, Formatting.None, JSONsettings));
        db.Child("Characters").Child(playerName).Child("blessings").SetRawJsonValueAsync(json);
    }

    public struct WorldProgress
    {
        public Dictionary<string, Dictionary<string, QuestVerticalProgress>> locations;

        public int highestTier()
        {

            return locations == null || locations.Count == 0 ? -1 : locations.Values.SelectMany(d => d.Values).Max(v => v.highestTier);

        }
        public int questTier(string locationId, string verticalId)
        {
            if (locations != null && locations.ContainsKey(locationId))
            {
                if (locations[locationId].ContainsKey(verticalId))
                {
                    return locations[locationId][verticalId].highestTier;
                }
            }
            return -1;
        }
    }
    public struct QuestVerticalProgress
    {
        public int highestTier;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
