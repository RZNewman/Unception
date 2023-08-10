using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    }

    public bool skipSpawn = false;
    public bool dissuadeCollapse = false;
    public NavLoad navType = NavLoad.None;
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
}
