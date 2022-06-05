using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedMaterials : NetworkBehaviour
{
    public List<string> enemyModels;
    public struct visualsSource
    {
        public Color color;
        public string modelName;
        public float lank;
    }

    public struct visualsBuilt
    {
        public Material[] materials;
        public GameObject modelPrefab;
    }

    public struct visualsData
    {
        public visualsSource source;
        public visualsBuilt built;
    }

    public delegate void OnVisuals(visualsData mat);

    Dictionary<int, visualsData> dataLookup = new Dictionary<int, visualsData>();
    Dictionary<int, List<OnVisuals>> pending = new Dictionary<int, List<OnVisuals>>();

    [Server]
    public int addVisuals(bool forPlayer = false)
    {
        visualsSource source;
        if (forPlayer)
        {
            source = new visualsSource
            {
                color = Color.white,
                modelName = "Lizard",
                lank =  1,
            };
        }
        else
        {
            Color c = new Color(Random.value, Random.value, Random.value);
            float lank = Random.Range(0, 2f);
            source = new visualsSource
            {
                color = c,
                lank = lank,
                modelName = enemyModels[Random.Range(0,enemyModels.Count)],
            };
        } 
        int index = dataLookup.Count;
        makeVisualInstance(index, source);
        RpcSyncVisuals(index, source);
        return index;
    }
    public void SyncVisuals(NetworkConnection conn)
    {
        foreach(int index in dataLookup.Keys)
        {
            TargetSyncVisuals(conn, index, dataLookup[index].source);
        }
    }

    [ClientRpc]
    void RpcSyncVisuals(int index, visualsSource s)
    {
        SyncVisuals(index, s);
    }
    [TargetRpc]
    void TargetSyncVisuals(NetworkConnection conn, int index, visualsSource s)
    {
        SyncVisuals(index, s);
    }
    [Client]
    void SyncVisuals(int index, visualsSource s)
    {
        makeVisualInstance(index, s);
        if (pending.ContainsKey(index))
        {
            foreach (OnVisuals callback in pending[index])
            {
                callback(dataLookup[index]);
            }
        }
    }
    void makeVisualInstance(int index, visualsSource s)
    {
        GameObject modelPrefab = Resources.Load("Models/" + s.modelName) as GameObject;
        Material[] mats = modelPrefab.GetComponent<UnitColorTarget>().getSource();
        Material[] outMats = new Material[mats.Length];
        for(int i = 0; i < mats.Length; i++)
        {
            Material m = new Material(mats[i]);
            m.SetColor(Shader.PropertyToID("_Color"), s.color);
            outMats[i] = m;
        }
        
        visualsData data = new visualsData();
        data.source = s;
        data.built = new visualsBuilt
        {
            materials = outMats,
            modelPrefab= modelPrefab,
        };
        dataLookup.Add(index, data);
    }
    public void getVisuals(int index, OnVisuals callback)
    {
        
        if (dataLookup.ContainsKey(index))
        {
            callback(dataLookup[index]);
        }
        else
        {
            addPending(index, callback);
        }
    }

    void addPending(int index, OnVisuals callback)
    {
        if (!pending.ContainsKey(index))
        {
            pending.Add(index, new List<OnVisuals>());
            
        }
        pending[index].Add(callback);
    }

}
