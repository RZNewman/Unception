using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedMaterials : NetworkBehaviour
{
    public Shader shader;
    struct builtMaterial
    {
        public Color source;
        public Material material;
    }

    public delegate void OnVisuals(Material mat);

    Dictionary<int, builtMaterial> materials = new Dictionary<int, builtMaterial>();
    Dictionary<int, List<OnVisuals>> pending = new Dictionary<int, List<OnVisuals>>();

    [Server]
    public int addVisuals(Color c)
    {
        int index = materials.Count;
        makeVisualInstance(index, c);
        RpcSyncVisuals(index, c);
        return index;
    }
    public void SyncVisuals(NetworkConnection conn)
    {
        foreach(int index in materials.Keys)
        {
            TargetSyncVisuals(conn, index, materials[index].source);
        }
    }

    [ClientRpc]
    void RpcSyncVisuals(int index, Color c)
    {
        SyncVisuals(index, c);
    }
    [TargetRpc]
    void TargetSyncVisuals(NetworkConnection conn, int index, Color c)
    {
        SyncVisuals(index, c);
    }
    [Client]
    void SyncVisuals(int index, Color c)
    {
        makeVisualInstance(index, c);
        if (pending.ContainsKey(index))
        {
            foreach (OnVisuals callback in pending[index])
            {
                callback(materials[index].material);
            }
        }
    }
    void makeVisualInstance(int index, Color c)
    {
        Material m = new Material(shader);
        m.SetColor(Shader.PropertyToID("_Color"), c);
        builtMaterial built = new builtMaterial();
        built.source = c;
        built.material = m;
        materials.Add(index, built);
    }
    public void getVisuals(int index, OnVisuals callback)
    {
        
        if (materials.ContainsKey(index))
        {
            callback(materials[index].material);
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
