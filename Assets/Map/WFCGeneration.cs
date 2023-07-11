using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static Utils;

public class WFCGeneration : MonoBehaviour
{
    public Vector3Int mapSize = new Vector3Int(100, 30, 100);
    public List<TileWeight> tiles;


    public struct TileWeight
    {
        public float weight;
        public WFCTile prefab;
    }
    public enum TileConnection
    {
        Flat = 1 << 0,
        Air = 1 << 1,
        Ground = 1 << 2,
        //RampLeft,
        //RampRight,
        //RampTop,
        //WallLeft,
        //WallRight,
        //FlatUnwalkable,
    }

    public static bool walkable(TileConnection conn)
    {
        return (conn & (TileConnection.Flat)) != 0;
    }
    public enum TileDirection
    {
        Forward,
        Right,
        Backward,
        Left,
        Up,
        Down,
    }
    static TileDirection opposite(TileDirection direction)
    {
        return direction switch
        {
            TileDirection.Forward => TileDirection.Backward,
            TileDirection.Backward => TileDirection.Forward,
            TileDirection.Left => TileDirection.Right,
            TileDirection.Right => TileDirection.Left,
            TileDirection.Up => TileDirection.Down,
            TileDirection.Down => TileDirection.Up,
            _ => TileDirection.Forward,
        };
    }

    public static TileDirection rotated(TileDirection direction, Rotation rotation)
    {
        return direction switch
        {

            TileDirection.Up => TileDirection.Up,
            TileDirection.Down => TileDirection.Down,
            TileDirection dir => (TileDirection)(((int)dir) + ((int)rotation) % 4),
        };
    }


    class WFCCell
    {
        public bool initialized = false;
        public bool collapsed = false;
        public int domainMask;
        public int forwardMask;
        public int rightMask;
        public int upMask;

        public void init(int fullDomain, int fullConnection)
        {
            initialized = true;
            domainMask = fullDomain;
            forwardMask = fullConnection;
            rightMask = fullConnection;
            upMask = fullConnection;
        }


    }

    WFCCell[,,] map;

    int fullConnectionMask()
    {
        return connectionDomain(EnumValues<TileConnection>());
    }

    int fullDomainMask()
    {
        int mask = 0;

        for (int i = 0; i < domainTiles.Count; i++)
        {
            mask |= 1 << i;
        }
        return mask;
    }

    public static int connectionDomain(IEnumerable<TileConnection> connections)
    {
        int mask = 0;

        foreach (int value in connections)
        {
            mask |= value;
        }
        return mask;
    }
    public enum Rotation
    {
        None,
        Quarter,
        Half,
        ThreeQuarters,

    }

    struct TileOption
    {
        public float weight;
        public WFCTile prefab;
        public Rotation rotation;
    }

    List<TileOption> domainTiles = new List<TileOption>();

    void makeDomain()
    {
        foreach (TileWeight tile in tiles)
        {
            List<Rotation> rots = new List<Rotation>() { Rotation.None };
            Dictionary<TileDirection, int> domains = tile.prefab.getDomains();
            if (domains[TileDirection.Forward] == domains[TileDirection.Backward]
                &&
                domains[TileDirection.Left] == domains[TileDirection.Right])
            {

                if (domains[TileDirection.Forward] != domains[TileDirection.Right])
                {
                    rots.Add(Rotation.Half);
                }
            }
            else
            {
                rots.Add(Rotation.Quarter);
                rots.Add(Rotation.Half);
                rots.Add(Rotation.ThreeQuarters);
            }
            foreach (Rotation r in rots)
            {
                domainTiles.Add(new TileOption
                {
                    rotation = r,
                    prefab = tile.prefab,
                    weight = tile.weight,
                });
            }
        }
        //Reverse in the sort function 
        domainTiles.Sort((a, b) => b.weight.CompareTo(a.weight));
    }
    static bool oneFlagSet(int i)
    {
        //http://aggregate.org/MAGIC/#Is%20Power%20of%202
        return i > 0 && (i & (i - 1)) == 0;
    }
    int selectFromDomain(int domain)
    {
        if (domain == 0)
        {
            throw new System.Exception("No domain to pick from");
        }
        if (oneFlagSet(domain))
        {
            return domain;
        }

        int processDomain = domain;
        List<(int, float)> indexWeights = new List<(int, float)>();
        float weightSum = 0;

        for (int i = 0; i < domainTiles.Count; i++)
        {
            if ((processDomain & 1) > 0)
            {
                TileOption opt = domainTiles[i];
                indexWeights.Add((i, opt.weight));
                weightSum += opt.weight;
            }
            processDomain = processDomain >> 1;
        }

        float pick = Random.value * weightSum;
        int selectedIndex = -1;

        foreach ((int index, float weight) in indexWeights)
        {
            if (weight <= pick)
            {
                selectedIndex = index;
                break;
            }
            else
            {
                pick -= weight;
            }
        }
        if (selectedIndex == -1)
        {
            selectedIndex = indexWeights[0].Item1;
        }

        //TODO maybe just spawn the tile here??
        return 1 << selectedIndex;

    }

    List<(TileOption, int)> optionsFromDomain(int domain)
    {
        List<(TileOption, int)> options = new List<(TileOption, int)>();
        int index = 0;
        while (domain > 1 && index < domainTiles.Count)
        {
            if ((domain & 1) > 0)
            {
                options.Add((domainTiles[index], index));
            }

            domain = domain >> 1;
            index++;
        }
        return options;
    }
    Dictionary<TileDirection, int> compositeDomains(int domain)
    {
        Dictionary<TileDirection, int> connections = new Dictionary<TileDirection, int>();
        foreach (TileDirection dir in EnumValues<TileDirection>())
        {
            connections[dir] = 0;
        }
        foreach ((TileOption opt, _) in optionsFromDomain(domain))
        {
            Dictionary<TileDirection, int> connectionsTile = opt.prefab.getDomains();
            foreach (TileDirection dir in EnumValues<TileDirection>())
            {
                connections[dir] |= connectionsTile[dir];
            }
        }

        return connections;
    }

    bool restrictDomain(Vector3Int loc)
    {
        WFCCell cell = map[loc.x, loc.y, loc.z];
        if (!cell.initialized)
        {
            return false;
        }
        if (cell.collapsed)
        {
            return false;
        }
        List<(TileOption, int)> options = optionsFromDomain(cell.domainMask);
        bool reduced = false;
        foreach ((TileOption opt, int index) in options)
        {
            Dictionary<TileDirection, int> domains = opt.prefab.getDomains();
            bool doesntFit = false;
            foreach (TileDirection dir in EnumValues<TileDirection>())
            {
                WFCCell negativeCell;
                switch (dir)
                {
                    case TileDirection.Forward:
                        if ((cell.forwardMask & domains[TileDirection.Forward]) == 0)
                        {

                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Right:
                        if ((cell.rightMask & domains[TileDirection.Right]) == 0)
                        {

                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Up:
                        if ((cell.upMask & domains[TileDirection.Up]) == 0)
                        {

                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Backward:
                        negativeCell = map[loc.x, loc.y, loc.z - 1];
                        if ((negativeCell.forwardMask & domains[TileDirection.Backward]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Left:
                        negativeCell = map[loc.x - 1, loc.y, loc.z];
                        if ((negativeCell.rightMask & domains[TileDirection.Left]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Down:
                        negativeCell = map[loc.x, loc.y - 1, loc.z];
                        if ((negativeCell.upMask & domains[TileDirection.Down]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                }
            }
        MatchFound:
            if (doesntFit)
            {
                cell.domainMask ^= 1 << index;
                reduced = true;
            }
        }

        if (cell.domainMask == 0)
        {
            throw new System.Exception("Domain reduced to 0");
        }

        return reduced;
    }

    delegate void CollapseEnqueue(Vector3Int loc);
    void collapseConnections(Vector3Int loc, CollapseEnqueue queue)
    {
        WFCCell cell = map[loc.x, loc.y, loc.z];
        Dictionary<TileDirection, int> domains = compositeDomains(cell.domainMask);
        foreach (TileDirection dir in EnumValues<TileDirection>())
        {
            WFCCell negativeCell;
            switch (dir)
            {
                case TileDirection.Forward:
                    if (cell.forwardMask != domains[TileDirection.Forward])
                    {
                        cell.forwardMask &= domains[TileDirection.Forward];
                        queue(loc + new Vector3Int(0, 0, 1));
                    }
                    break;
                case TileDirection.Right:
                    if (cell.rightMask != domains[TileDirection.Right])
                    {
                        cell.rightMask &= domains[TileDirection.Right];
                        queue(loc + new Vector3Int(1, 0, 0));
                    }
                    break;
                case TileDirection.Up:
                    if (cell.upMask != domains[TileDirection.Up])
                    {
                        cell.upMask &= domains[TileDirection.Up];
                        queue(loc + new Vector3Int(0, 1, 0));
                    }
                    break;
                case TileDirection.Backward:
                    negativeCell = map[loc.x, loc.y, loc.z - 1];
                    if (negativeCell.forwardMask != domains[TileDirection.Backward])
                    {
                        negativeCell.forwardMask &= domains[TileDirection.Backward];
                        queue(loc + new Vector3Int(0, 0, -1));
                    }
                    break;
                case TileDirection.Left:
                    negativeCell = map[loc.x - 1, loc.y, loc.z];
                    if (negativeCell.rightMask != domains[TileDirection.Left])
                    {
                        negativeCell.rightMask &= domains[TileDirection.Left];
                        queue(loc + new Vector3Int(-1, 0, 0));
                    }
                    break;
                case TileDirection.Down:
                    negativeCell = map[loc.x, loc.y - 1, loc.z];
                    if (negativeCell.upMask != domains[TileDirection.Down])
                    {
                        negativeCell.upMask &= domains[TileDirection.Down];
                        queue(loc + new Vector3Int(0, -1, 0));
                    }
                    break;
            }
        }
    }
    public void init()
    {
        map = new WFCCell[mapSize.x, mapSize.y, mapSize.z];
        makeDomain();
    }

    public IEnumerator collapseCells(int xx, int yy, int zz, int width, int height, int length)
    {
        SimplePriorityQueue<Vector3Int> collapseQueue = new SimplePriorityQueue<Vector3Int>();

        int fullDomain = fullDomainMask();
        int fullConnection = fullConnectionMask();
        for (int x = xx; x < xx + width; x++)
        {
            for (int y = yy; y < yy + height; y++)
            {
                for (int z = zz; z < zz + length; z++)
                {
                    map[x, y, z].init(fullDomain, fullConnection);
                    collapseQueue.Enqueue(new Vector3Int(x, y, z), domainTiles.Count);
                }
            }
        }

        Queue<Vector3Int> propagation = new Queue<Vector3Int>();


        while (collapseQueue.Count > 0)
        {
            Vector3Int coords = collapseQueue.Dequeue();
            WFCCell cell = map[coords.x, coords.y, coords.z];
            cell.domainMask = selectFromDomain(cell.domainMask);
            cell.collapsed = true;
            collapseConnections(coords, propagation.Enqueue);
            while (propagation.Count > 0)
            {
                Vector3Int propLocation = propagation.Dequeue();
                if (restrictDomain(propLocation))
                {
                    collapseConnections(propLocation, propagation.Enqueue);
                }

            }
            yield return null;
        }
    }

}
