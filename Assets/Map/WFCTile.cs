using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;
using static WFCGeneration;

public class WFCTile : MonoBehaviour
{
    [System.Serializable]
    public struct ConnectionOptions
    {
        public TileDirection direction;
        public List<TileConnection> connections;
    }
    public enum NavLoad
    {
        None,
        This,
        Beneath,
        ThisAndBeneath,
    }

    public bool skipSpawn = false;
    public bool dissuadeCollapse = false;
    public NavLoad navType = NavLoad.None;
    public List<WFCTileOption> alternatives = new List<WFCTileOption>();
    public List<ConnectionOptions> adjacencies;


    public Dictionary<TileDirection, int> getDomains(Rotation rotation = Rotation.None)
    {
        Dictionary<TileDirection, int> domains = new Dictionary<TileDirection, int>();
        foreach (ConnectionOptions connection in adjacencies)
        {
            int connD = connectionDomain(connection.connections);
            domains[rotated(connection.direction, rotation)] = connD;
        }
        if (domains.Count != 6)
        {
            throw new System.Exception("Tile " + name + ", Rotation " + rotation + " without 6 directions: had " + domains.Count);
        }
        return domains;
    }


    public GameObject getPrefabToSpawn()
    {
        if (alternatives.Count == 0)
        {
            return gameObject;
        }

        List<GameObject> tiles = new List<GameObject>();
        foreach(WFCTileOption opt in alternatives)
        {
            tiles.Add(opt.gameObject);
        }
        tiles.Add(gameObject);
        return tiles.RandomItemWeighted((t) => { WFCTileOption opt = t.GetComponent<WFCTileOption>(); return opt ? opt.weightPercent : 100; });
    }
}
