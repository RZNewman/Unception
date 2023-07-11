using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WFCGeneration;

public class WFCTile : MonoBehaviour
{
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
            domains[rotated(connection.direction, rotation)] = connectionDomain(connection.connections);
        }
        return domains;
    }
}
