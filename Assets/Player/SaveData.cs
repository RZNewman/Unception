using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Newtonsoft.Json;


public class SaveData : MonoBehaviour
{
    DatabaseReference db;
    JsonSerializerSettings settings;
    // Start is called before the first frame update
    void Start()
    {
        
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

    public void saveItem(AttackBlock block)
    {
        
        string json = santitizeJson(JsonConvert.SerializeObject(block, Formatting.None, settings));
        Debug.Log(json);
        //AttackBlock bloc = JsonConvert.DeserializeObject<AttackBlock>(json, settings);
        db.SetRawJsonValueAsync(json);
    }
}
