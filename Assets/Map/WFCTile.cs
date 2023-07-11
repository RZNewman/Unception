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

    public List<ConnectionOptions> adjacencies;

    public Dictionary<TileDirection, int> getDomains(Rotation rotation = Rotation.None)
    {
        Dictionary<TileDirection, int> domains = new Dictionary<TileDirection, int>();
        foreach (ConnectionOptions connection in adjacencies)
        {
            int connD = connectionDomain(connection.connections);
            domains[rotated(connection.direction, rotation)] = connD;
        }
        return domains;
    }
}
